using Application.DTOs;
using Application.Services.Implementations;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

/// <summary>Checks for job titles, changing users and deleting users. Uses the same temporary database as Program.cs.</summary>
static class UserChecks
{
    public static async Task RunAsync(AppDbContext db, Guid adminId, Action<bool, string> check,
        Func<Func<Task>, string, Task> conflict, Func<Func<Task>, string, Task> invalid)
    {
        var users = new UserService(new UserRepository(db), new RoleRepository(db));
        var tokens = new AccessTokenValidator(new UserRepository(db));

        // Job titles: the role follows from the job title.
        var seededAdmin = (await users.GetByIdAsync(adminId))!;
        check(seededAdmin.JobTitle == JobTitles.Facilitair && seededAdmin.Role.Name == RoleNames.Administrator, "Testbeheerder is Facilitair na de migratie");
        check((await users.GetAllAsync()).All(user => JobTitles.Find(user.JobTitle) == user.JobTitle), "Iedereen heeft een vaste functie");
        var facility = await users.CreateAsync(new CreateUserDto { Name = "Fien Facilitair", JobTitle = "facilitair", Email = " Fien@Actievadis.nl ", Password = "Welkom123!" });
        check(facility.JobTitle == JobTitles.Facilitair && facility.Role.Name == RoleNames.Administrator, "Facilitair wordt automatisch beheerder");
        check(facility.Email == "fien@actievadis.nl", "E-mailadres wordt genormaliseerd");
        var sales = await users.CreateAsync(new CreateUserDto { Name = "Sam Sales", JobTitle = JobTitles.Sales, Email = "sam@actievadis.nl", Password = "Welkom123!" });
        check(sales.Role.Name == RoleNames.User, "Andere functies zijn medewerker");
        await invalid(() => users.CreateAsync(new CreateUserDto { Name = "Onbekend", JobTitle = "Marketing", Email = "onbekend@actievadis.nl", Password = "Welkom123!" }), "Functie buiten de lijst geweigerd");
        await conflict(() => users.CreateAsync(new CreateUserDto { Name = "Dubbel", JobTitle = JobTitles.HR, Email = "sam@actievadis.nl", Password = "Welkom123!" }), "Dubbel e-mailadres geweigerd");

        // Changing users: name, email address and job title (and with it the role).
        var promoted = await users.UpdateAsync(sales.Id, new UpdateUserDto { Name = "Sam de Vries", JobTitle = JobTitles.Facilitair, Email = "sam.devries@actievadis.nl" }, adminId);
        check(promoted!.Name == "Sam de Vries" && promoted.Email == "sam.devries@actievadis.nl" && promoted.Role.Name == RoleNames.Administrator, "Gebruiker volledig wijzigen; Facilitair wordt beheerder");
        var demoted = await users.UpdateAsync(sales.Id, new UpdateUserDto { Name = "Sam de Vries", JobTitle = JobTitles.Teamlead, Email = "sam.devries@actievadis.nl" }, adminId);
        check(demoted!.Role.Name == RoleNames.User, "Andere functie neemt beheerrechten af");
        check(!await tokens.IsCurrentAsync(sales.Id, RoleNames.Administrator), "Oud beheerderstoken vervalt na functiewijziging");
        await conflict(() => users.UpdateAsync(facility.Id, new UpdateUserDto { Name = facility.Name, JobTitle = JobTitles.HR, Email = facility.Email }, facility.Id), "Eigen beheerrechten afnemen geweigerd");
        await conflict(() => users.UpdateAsync(sales.Id, new UpdateUserDto { Name = "Sam", JobTitle = JobTitles.Teamlead, Email = "fien@actievadis.nl" }, adminId), "Wijzigen naar bestaand e-mailadres geweigerd");
        await invalid(() => users.UpdateAsync(sales.Id, new UpdateUserDto { Name = "Sam", JobTitle = "Stagiair", Email = "sam.devries@actievadis.nl" }, adminId), "Wijzigen naar onbekende functie geweigerd");
        check(await users.UpdateAsync(Guid.NewGuid(), new UpdateUserDto { Name = "X", JobTitle = JobTitles.HR, Email = "x@actievadis.nl" }, adminId) is null, "Onbekende gebruiker wijzigen");

        // Two administrators (the seeded one and Fien): one may lose Facilitair, the last one may not.
        var formerAdmin = await users.UpdateAsync(facility.Id, new UpdateUserDto { Name = facility.Name, JobTitle = JobTitles.Directeur, Email = facility.Email }, adminId);
        check(formerAdmin!.Role.Name == RoleNames.User, "Beheerder kan een andere beheerder een andere functie geven");
        await conflict(() => users.UpdateAsync(adminId, new UpdateUserDto { Name = seededAdmin.Name, JobTitle = JobTitles.HR, Email = seededAdmin.Email }, facility.Id), "Laatste beheerder houdt Facilitair");

        await new AdministratorBootstrapService(new UserRepository(db), new RoleRepository(db)).BootstrapAsync("sam.devries@actievadis.nl");
        var bootstrapped = (await users.GetByIdAsync(sales.Id))!;
        check(bootstrapped.JobTitle == JobTitles.Facilitair && bootstrapped.Role.Name == RoleNames.Administrator, "Beheerder via bootstrap krijgt functie Facilitair");

        // Deleting users: soft delete, future registrations disappear, past ones stay.
        var activities = new ActivityService(new ActivityRepository(db), db);
        var authentication = new AuthenticationService(new UserRepository(db));
        var leaver = await users.CreateAsync(new CreateUserDto { Name = "Lars Vertrekker", JobTitle = JobTitles.Afstudeerder, Email = "lars@actievadis.nl", Password = "Welkom123!" });
        var future = await activities.CreateActivityAsync(new ActivityInputDto { Name = "Toekomst", Location = "Kantoor", Description = "Nog niet begonnen",
            StartTime = DateTime.Now.AddDays(3), EndTime = DateTime.Now.AddDays(3).AddHours(2), MaxParticipants = 1 });
        await activities.RegisterAsync(future.Id, leaver.Id);
        var history = await activities.CreateActivityAsync(new ActivityInputDto { Name = "Geschiedenis", Location = "Kantoor", Description = "Al geweest",
            StartTime = DateTime.Now.AddDays(-3), EndTime = DateTime.Now.AddDays(-3).AddHours(2) });
        db.Registrations.Add(new Registration { ActivityId = history.Id, UserId = leaver.Id, RegisteredAt = DateTime.UtcNow.AddDays(-4) });
        await db.SaveChangesAsync();

        await conflict(() => users.DeleteAsync(adminId, adminId), "Eigen account verwijderen geweigerd");
        check(await users.DeleteAsync(leaver.Id, adminId), "Gebruiker verwijderen");
        check(await users.GetByIdAsync(leaver.Id) is null && (await users.GetAllAsync()).All(user => user.Id != leaver.Id), "Verwijderde gebruiker is onzichtbaar");
        check(await db.Users.IgnoreQueryFilters().AnyAsync(user => user.Id == leaver.Id && user.DeletedAt != null), "Verwijderde gebruiker blijft bewaard in de database");
        check(await authentication.AuthenticateAsync("lars@actievadis.nl", "Welkom123!") is null, "Verwijderde gebruiker kan niet inloggen");
        check(!await tokens.IsCurrentAsync(leaver.Id, RoleNames.User), "Token van verwijderde gebruiker vervalt");
        check((await activities.GetActivityByIdAsync(future.Id))!.RegistrationCount == 0, "Toekomstige inschrijving vervalt en de plek komt vrij");
        var historyRegistrations = await activities.GetActivityRegistrationsAsync(history.Id);
        check(historyRegistrations.Any(registration => registration.ParticipantName == "Lars Vertrekker" && registration.ParticipantDeleted), "Eerdere deelname blijft bewaard");
        check(!await users.DeleteAsync(leaver.Id, adminId), "Verwijderde gebruiker kan niet nog eens worden verwijderd");
        var returning = await users.CreateAsync(new CreateUserDto { Name = "Lars Terug", JobTitle = JobTitles.Ontwikkelaar1, Email = "lars@actievadis.nl", Password = "Welkom123!" });
        check(returning.Id != leaver.Id, "E-mailadres van verwijderde gebruiker is opnieuw te gebruiken");
        check((await authentication.AuthenticateAsync("lars@actievadis.nl", "Welkom123!"))?.Id == returning.Id, "Nieuw account logt in met het hergebruikte e-mailadres");

        // Sam (bootstrap) and the seeded account are administrators: one may go, the last one may not.
        check(await users.DeleteAsync(sales.Id, adminId), "Beheerder verwijderen als er nog een beheerder is");
        await conflict(() => users.DeleteAsync(adminId, facility.Id), "Laatste beheerder verwijderen geweigerd");
    }
}
