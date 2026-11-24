using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using API.Controllers;
using Application.DTOs;
using Application.Security;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.Validation;

/// <summary>Checks for visitors without an account ("gasten"). Uses the same temporary database as Program.cs.</summary>
static class VisitorChecks
{
    public static async Task RunAsync(AppDbContext db, Guid employeeId, Action<bool, string> check,
        Func<Func<Task>, string, Task> conflict, Func<Func<Task>, string, Task> invalid)
    {
        var activities = new ActivityService(new ActivityRepository(db), db);
        var mails = new RecordingEmailSender();
        var visitors = new VisitorRegistrationService(activities, mails,
            Options.Create(new VisitorEmailOptions { CancellationPageUrl = "http://localhost/uitschrijven" }),
            NullLogger<VisitorRegistrationService>.Instance);

        // Registering as a visitor: counts as a participant, may bring a +1, gets a cancellation code.
        var openDay = await activities.CreateActivityAsync(new ActivityInputDto { Name = "Open dag", Location = "Kantoor", Description = "Ook voor gasten",
            StartTime = DateTime.Now.AddDays(5), EndTime = DateTime.Now.AddDays(5).AddHours(3), MaxParticipants = 2, AllowsGuest = true });
        var visitor = new VisitorRegistrationDto { Name = "  Gijs Gast ", Email = " Gijs@Example.com ", Phone = "06 12345678", BringsGuest = true, Comment = "Ik kom met mijn partner" };
        var registered = await visitors.RegisterAsync(openDay.Id, visitor);
        check(registered!.Activity.RegistrationCount == 1 && registered.Activity.PlusOneCount == 1, "Gast schrijft zich in met een +1 en telt mee als deelnemer");
        check(Regex.IsMatch(registered.CancellationCode, "^[A-Z2-9]{4}-[A-Z2-9]{4}$"), "Gast krijgt een uitschrijfcode");
        var stored = await db.Registrations.AsNoTracking().SingleAsync(r => r.ActivityId == openDay.Id && r.UserId == null);
        check(stored.VisitorName == "Gijs Gast" && stored.VisitorEmail == "gijs@example.com" && stored.VisitorPhone == "06 12345678", "Gegevens van de gast worden bewaard");
        check(stored.CancellationCodeHash is not null && !stored.CancellationCodeHash.Contains(registered.CancellationCode.Replace("-", "")), "Uitschrijfcode wordt alleen als hash bewaard");
        var mail = mails.Registrations.SingleOrDefault();
        check(mail is not null && mail.To == "gijs@example.com" && mail.VisitorName == "Gijs Gast" && mail.CancellationCode == registered.CancellationCode
            && mail.ActivityName == "Open dag" && mail.BringsGuest && mail.CancellationPageUrl == "http://localhost/uitschrijven", "Bevestigingsmail krijgt alle gegevens mee");

        await conflict(() => visitors.RegisterAsync(openDay.Id, new VisitorRegistrationDto { Name = "Gijs", Email = "GIJS@example.com", Phone = "0612345678" }), "Gast kan zich niet twee keer inschrijven");
        await conflict(() => visitors.RegisterAsync(openDay.Id, new VisitorRegistrationDto { Name = "Sanne", Email = "sanne.jansen@actievadis.nl", Phone = "0612345678" }), "E-mailadres van een account moet inloggen");
        await invalid(() => visitors.RegisterAsync(openDay.Id, new VisitorRegistrationDto { Name = " ", Email = "leeg@example.com", Phone = "0612345678" }), "Gast zonder naam geweigerd");
        await invalid(() => visitors.RegisterAsync(openDay.Id, new VisitorRegistrationDto { Name = "Tess", Email = "test@test", Phone = "0612345678" }), "E-mailadres zonder extensie (test@test) geweigerd");
        check(!EmailFormat.IsValid("test@test") && !EmailFormat.IsValid("test@test.") && !EmailFormat.IsValid("@test.nl") && !EmailFormat.IsValid("te st@test.nl")
            && EmailFormat.IsValid("test@test.nl") && EmailFormat.IsValid("naam@bedrijf.com") && EmailFormat.IsValid(" Naam@Sub.Bedrijf.org "), "E-mailadres moet een extensie als .nl of .com hebben");
        var formErrors = new List<ValidationResult>();
        var form = new VisitorRegistrationDto { Name = "Tess", Email = "test@test", Phone = "0612345678" };
        Validator.TryValidateObject(form, new ValidationContext(form), formErrors, true);
        check(formErrors.Any(error => error.ErrorMessage == EmailFormat.ErrorMessage), "Formulier meldt een e-mailadres zonder extensie");
        check(await visitors.RegisterAsync(Guid.NewGuid(), new VisitorRegistrationDto { Name = "X", Email = "x@example.com", Phone = "0612345678" }) is null, "Gast voor onbekende activiteit");

        // Visitors and employees share the maximum (2 here).
        check((await activities.RegisterAsync(openDay.Id, employeeId))!.RegistrationCount == 2, "Medewerker en gast vullen samen het maximum");
        await conflict(() => visitors.RegisterAsync(openDay.Id, new VisitorRegistrationDto { Name = "Vera", Email = "vera@example.com", Phone = "0612345678" }), "Gast geweigerd als de activiteit vol is");
        var list = (await activities.GetActivityRegistrationsAsync(openDay.Id)).ToList();
        check(list.Count == 2
            && list.Any(r => r.IsVisitor && r.ParticipantName == "Gijs Gast" && r.VisitorEmail == "gijs@example.com" && r.VisitorPhone == "06 12345678" && r.BringsGuest)
            && list.Any(r => !r.IsVisitor && r.VisitorEmail == null), "Organisator ziet gasten met contactgegevens");

        var noPlusOnes = await activities.CreateActivityAsync(new ActivityInputDto { Name = "Lezing", Location = "Kantoor", Description = "Zonder +1",
            StartTime = DateTime.Now.AddDays(6), EndTime = DateTime.Now.AddDays(6).AddHours(1) });
        await conflict(() => visitors.RegisterAsync(noPlusOnes.Id, new VisitorRegistrationDto { Name = "Pim", Email = "pim@example.com", Phone = "0612345678", BringsGuest = true }), "+1 van gast geweigerd wanneer niet toegestaan");

        // Unregistering with e-mail address and code.
        check(await visitors.CancelAsync(new CancelVisitorRegistrationDto { Email = "gijs@example.com", Code = "AAAA-AAAA" }) is null, "Verkeerde code schrijft niet uit");
        check(await visitors.CancelAsync(new CancelVisitorRegistrationDto { Email = "ander@example.com", Code = registered.CancellationCode }) is null, "Code werkt niet met een ander e-mailadres");
        var typedCode = registered.CancellationCode.Replace("-", " ").ToLowerInvariant();
        var cancelled = await visitors.CancelAsync(new CancelVisitorRegistrationDto { Email = " GIJS@example.com", Code = typedCode });
        check(cancelled?.ActivityName == "Open dag", "Gast schrijft zich uit met e-mail en code (hoofdletters en streepje maken niet uit)");
        check((await activities.GetActivityByIdAsync(openDay.Id))!.RegistrationCount == 1, "Uitschrijven maakt de plek vrij");
        var cancellationMail = mails.Cancellations.SingleOrDefault();
        check(cancellationMail is not null && cancellationMail.To == "gijs@example.com" && cancellationMail.VisitorName == "Gijs Gast" && cancellationMail.ActivityName == "Open dag", "Bevestigingsmail na uitschrijven");
        check(await visitors.CancelAsync(new CancelVisitorRegistrationDto { Email = "gijs@example.com", Code = registered.CancellationCode }) is null, "Een code werkt maar één keer");
        check((await visitors.RegisterAsync(openDay.Id, visitor))!.Activity.RegistrationCount == 2, "Gast kan zich opnieuw inschrijven");

        // Once the activity has started, visitors can no longer register or unregister.
        var started = await activities.CreateActivityAsync(new ActivityInputDto { Name = "Al bezig", Location = "Kantoor", Description = "Is begonnen",
            StartTime = DateTime.Now.AddHours(-1), EndTime = DateTime.Now.AddHours(1) });
        db.Registrations.Add(new Registration { ActivityId = started.Id, VisitorName = "Laat", VisitorEmail = "laat@example.com", VisitorPhone = "0612345678",
            CancellationCodeHash = CancellationCodes.Hash("ABCD-EFGH"), RegisteredAt = DateTime.UtcNow.AddDays(-1) });
        await db.SaveChangesAsync();
        await conflict(() => visitors.CancelAsync(new CancelVisitorRegistrationDto { Email = "laat@example.com", Code = "ABCD-EFGH" }), "Gast kan zich niet uitschrijven na de start");
        await conflict(() => visitors.RegisterAsync(started.Id, new VisitorRegistrationDto { Name = "Te laat", Email = "telaat@example.com", Phone = "0612345678" }), "Gast kan zich niet inschrijven na de start");

        // A failing mail service never undoes the registration: the website shows the code as well.
        var withoutMail = new VisitorRegistrationService(activities, new FailingEmailSender(), Options.Create(new VisitorEmailOptions()), NullLogger<VisitorRegistrationService>.Instance);
        var registeredWithoutMail = await withoutMail.RegisterAsync(noPlusOnes.Id, new VisitorRegistrationDto { Name = "Mo", Email = "mo@example.com", Phone = "0612345678" });
        check(registeredWithoutMail is not null && await db.Registrations.AnyAsync(r => r.VisitorEmail == "mo@example.com"), "Inschrijving blijft staan als de mail mislukt");

        // Anyone may look at activities and register as a visitor, without logging in.
        foreach (var name in new[] { "GetAllActivities", "GetActivityById" })
            check(typeof(ActivitiesController).GetMethod(name)!.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any(), $"{name} kan zonder inloggen");
        check(typeof(VisitorRegistrationsController).GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any(), "Gastinschrijving kan zonder inloggen");

        // The database itself refuses a registration that belongs to nobody.
        db.Registrations.Add(new Registration { ActivityId = noPlusOnes.Id, RegisteredAt = DateTime.UtcNow });
        var refused = false;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { refused = true; }
        db.ChangeTracker.Clear();
        check(refused, "Database weigert een inschrijving zonder medewerker of gastgegevens");
    }

    private sealed class RecordingEmailSender : IVisitorEmailSender
    {
        public List<VisitorRegistrationEmail> Registrations { get; } = [];
        public List<VisitorCancellationEmail> Cancellations { get; } = [];

        public Task SendRegistrationConfirmationAsync(VisitorRegistrationEmail email)
        {
            Registrations.Add(email);
            return Task.CompletedTask;
        }

        public Task SendCancellationConfirmationAsync(VisitorCancellationEmail email)
        {
            Cancellations.Add(email);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingEmailSender : IVisitorEmailSender
    {
        public Task SendRegistrationConfirmationAsync(VisitorRegistrationEmail email) => throw new InvalidOperationException("Mail server is down.");
        public Task SendCancellationConfirmationAsync(VisitorCancellationEmail email) => throw new InvalidOperationException("Mail server is down.");
    }
}
