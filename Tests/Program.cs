using Application.Services.Implementations;
using Shared.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Constants;
using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Controllers;

var adminId = Guid.Parse("20000000-0000-4000-8000-000000000001");
var userId = Guid.Parse("20000000-0000-4000-8000-000000000002");
var otherUserId = Guid.Parse("20000000-0000-4000-8000-000000000003");
var path = Path.Combine(Path.GetTempPath(), $"actievadis-tests-{Guid.NewGuid()}.db");
try
{
    var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={path}").Options;
    await using var db = new AppDbContext(options);
    await db.Database.MigrateAsync();
    Check(!db.Database.HasPendingModelChanges(), "Migraties komen overeen met het datamodel");
    await UserChecks.RunAsync(db, adminId, Check, (action, name) => Conflict(action, name), (action, name) => Conflict(action, name, true));
    await VisitorChecks.RunAsync(db, userId, Check, (action, name) => Conflict(action, name), (action, name) => Conflict(action, name, true));
    var service = new ActivityService(new ActivityRepository(db), db);
    var created = await service.CreateActivityAsync(new ActivityInputDto {
        Name = "Test", Location = "Kantoor", Description = "Testactiviteit", StartTime = DateTime.Now.AddDays(1),
        EndTime = DateTime.Now.AddDays(1).AddHours(1), MaxParticipants = 1 });
    var registered = await service.RegisterAsync(created.Id, userId);
    Check(registered!.RegistrationCount == 1, "Inschrijving telt mee");
    Check(await service.IsRegisteredAsync(created.Id, userId), "Eigen inschrijving opvragen");
    await Conflict(() => service.RegisterAsync(created.Id, userId), "Dubbele inschrijving geweigerd");
    await Conflict(() => service.RegisterAsync(created.Id, otherUserId), "Volle activiteit geweigerd");
    await Conflict(() => service.UpdateActivityAsync(created.Id, new ActivityInputDto {
        Name = "Test", Location = "Kantoor", Description = "Test", StartTime = created.StartTime,
        EndTime = created.EndTime, MaxParticipants = 0 }), "Ongeldig maximum geweigerd", true);
    var past = await service.CreateActivityAsync(new ActivityInputDto { Name = "Voorbij", Location = "Kantoor",
        Description = "Voorbije activiteit", StartTime = DateTime.Now.AddDays(-2), EndTime = DateTime.Now.AddDays(-1) });
    await Conflict(() => service.RegisterAsync(past.Id, userId), "Voorbije activiteit weigert inschrijving");
    Check(await service.RegisterAsync(Guid.NewGuid(), userId) is null, "Onbekende activiteit");
    var updated = await service.UpdateActivityAsync(created.Id, new ActivityInputDto {
        Name = "Gewijzigd", Location = "Andere locatie", Description = "Gewijzigde informatie", StartTime = created.StartTime,
        EndTime = created.EndTime, MaxParticipants = 2 });
    Check(updated!.Name == "Gewijzigd" && updated.RegistrationCount == 1, "Wijzigen behoudt inschrijvingen");
    db.ChangeTracker.Clear();
    Check((await service.GetActivityByIdAsync(created.Id))!.RegistrationCount == 1, "Inschrijvingen laden uit database");
    Check(await service.DeleteActivityAsync(created.Id), "Verwijderen");
    Check(!await db.Registrations.AnyAsync(r => r.ActivityId == created.Id), "Inschrijvingen worden mee verwijderd");
    Check(await service.GetActivityByIdAsync(created.Id) is null, "Verwijderd uit overzicht");
    foreach (var name in new[] { "CreateActivity", "UpdateActivity", "DeleteActivity" })
        Check(typeof(ActivitiesController).GetMethod(name)!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Any(a => a.Roles == RoleNames.Administrator), $"{name} vereist beheerder");
    var guests = await service.CreateActivityAsync(new ActivityInputDto {
        Name = "Met +1", Location = "Kantoor", Description = "Activiteit met gasten", StartTime = DateTime.Now.AddDays(2),
        EndTime = DateTime.Now.AddDays(2).AddHours(1), MaxParticipants = 2, AllowsGuest = true });
    var administratorRegistration = await service.RegisterAsync(guests.Id, adminId, true);
    Check(administratorRegistration!.RegistrationCount == 1 && administratorRegistration.PlusOneCount == 1, "+1 telt niet mee als deelnemer");
    var guestStatus = await service.GetRegistrationAsync(guests.Id, adminId);
    Check(guestStatus.IsRegistered && guestStatus.BringsGuest, "+1 blijft bewaard bij herladen");
    Check((await service.RegisterAsync(guests.Id, otherUserId, true))!.RegistrationCount == 2, "+1 neemt geen plek in");
    await Conflict(() => service.RegisterAsync(guests.Id, userId), "Maximum telt alleen deelnemers");
    Check((await service.UpdateActivityAsync(guests.Id, new ActivityInputDto {
        Name = guests.Name, Location = guests.Location, Description = guests.Description, StartTime = guests.StartTime,
        EndTime = guests.EndTime, MaxParticipants = 2, AllowsGuest = true }))!.PlusOneCount == 2, "Maximum gelijk aan deelnemers mag ondanks +1's");
    await Conflict(() => service.UpdateActivityAsync(guests.Id, new ActivityInputDto {
        Name = guests.Name, Location = guests.Location, Description = guests.Description, StartTime = guests.StartTime,
        EndTime = guests.EndTime, MaxParticipants = 1, AllowsGuest = true }), "Maximum mag niet onder het aantal deelnemers");
    var limited = await service.CreateActivityAsync(new ActivityInputDto {
        Name = "Laatste plek", Location = "Kantoor", Description = "Nog een plek", StartTime = DateTime.Now.AddDays(2),
        EndTime = DateTime.Now.AddDays(2).AddHours(1), MaxParticipants = 1, AllowsGuest = true });
    Check((await service.RegisterAsync(limited.Id, userId, true))!.RegistrationCount == 1, "+1 mag mee op de laatste plek");
    var noGuests = await service.CreateActivityAsync(new ActivityInputDto {
        Name = "Geen +1", Location = "Kantoor", Description = "Geen gasten", StartTime = DateTime.Now.AddDays(2),
        EndTime = DateTime.Now.AddDays(2).AddHours(1) });
    await Conflict(() => service.RegisterAsync(noGuests.Id, otherUserId, true), "+1 geweigerd wanneer niet toegestaan");

    // Changing a registration afterwards: +1, comment and unregistering.
    var withoutGuest = await service.UpdateRegistrationAsync(guests.Id, otherUserId, false, "  Ik kom iets later  ");
    Check(withoutGuest!.RegistrationCount == 2 && withoutGuest.PlusOneCount == 1, "+1 achteraf weghalen");
    var changedStatus = await service.GetRegistrationAsync(guests.Id, otherUserId);
    Check(!changedStatus.BringsGuest && changedStatus.Comment == "Ik kom iets later", "Opmerking achteraf wijzigen");
    Check((await service.UpdateRegistrationAsync(guests.Id, otherUserId, true, null))!.PlusOneCount == 2, "+1 achteraf toevoegen");
    Check(await service.UpdateRegistrationAsync(guests.Id, userId, true, null) is null, "Wijzigen zonder inschrijving");
    Check((await service.RegisterAsync(noGuests.Id, otherUserId))!.PlusOneCount == 0, "Inschrijven zonder +1");
    await Conflict(() => service.UpdateRegistrationAsync(noGuests.Id, otherUserId, true, null), "+1 achteraf geweigerd wanneer niet toegestaan");
    Check(await service.UnregisterAsync(guests.Id, otherUserId), "Uitschrijven");
    Check(!await service.IsRegisteredAsync(guests.Id, otherUserId), "Uitschrijven verwijdert de inschrijving");
    Check(!await service.UnregisterAsync(guests.Id, otherUserId), "Uitschrijven zonder inschrijving");
    Check((await service.RegisterAsync(guests.Id, otherUserId))!.RegistrationCount == 2, "Opnieuw inschrijven na uitschrijven");
    db.Registrations.Add(new Registration { ActivityId = past.Id, UserId = userId, RegisteredAt = DateTime.UtcNow.AddDays(-3) });
    await db.SaveChangesAsync();
    await Conflict(() => service.UnregisterAsync(past.Id, userId), "Uitschrijven geweigerd na de start");
    await Conflict(() => service.UpdateRegistrationAsync(past.Id, userId, false, "Te laat"), "Wijzigen geweigerd na de start");
    var registerRoles = typeof(ActivitiesController).GetMethod("Register")!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>();
    Check(!registerRoles.Any(a => !string.IsNullOrWhiteSpace(a.Roles)), "Inschrijven heeft geen beperking tot één rol");
    foreach (var name in new[] { "UpdateRegistration", "Unregister" })
        Check(!typeof(ActivitiesController).GetMethod(name)!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Any(a => !string.IsNullOrWhiteSpace(a.Roles)), $"{name} kan door iedere ingelogde gebruiker");
    Check((await service.GetActivitiesForUserAsync(adminId)).Any(a => a.Id == guests.Id), "Inschrijving verschijnt in Mijn activiteiten");
    Console.WriteLine("Alle controles geslaagd.");
}
finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); File.Delete(path); }

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine($"OK: {name}");
}
static async Task Conflict(Func<Task> action, string name, bool validation = false)
{
    try { await action(); }
    catch (ConflictException) { Console.WriteLine($"OK: {name}"); return; }
    catch (ApplicationValidationException) when (validation) { Console.WriteLine($"OK: {name}"); return; }
    throw new Exception(name);
}
