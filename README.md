# Actievadis – activiteiten voor medewerkers

Een applicatie om bedrijfsactiviteiten te plannen en je ervoor in te schrijven. Het bestaat uit een **ASP.NET Core Web API** en een **Blazor**-frontend (.NET 8).

## Functies
- Inloggen met JWT-authenticatie
- Rollen: **Beheerder** en **Gebruiker**. Medewerkers met de functie Facilitair worden automatisch beheerder.
- Activiteiten bekijken, aanmaken, wijzigen en verwijderen (beheerder), met een afbeelding uploaden
- Inschrijven voor een activiteit, met een +1 en een opmerking. Inschrijving wijzigen of uitschrijven tot de activiteit begint.
- Een maximum aantal deelnemers, en controles op volle of voorbije activiteiten
- Gasten zonder account kunnen zich inschrijven en uitschrijven met een code die ze per e-mail krijgen
- Pagina "Mijn activiteiten"
- Gebruikersbeheer voor beheerders, met zacht verwijderen. De laatste beheerder kan niet verwijderd worden.
- Zoeken op locatie, prijs, met eten, aankomend en beschikbare plekken

## Architectuur
| Project | Inhoud |
| --- | --- |
| `Domain` | Entities (`Activity`, `Registration`, `User`, `Role`) en constanten |
| `Application` | Services, DTO's en wachtwoord-hashing |
| `Infrastructure` | `AppDbContext`, migrations en repositories |
| `Shared` | DTO's en validatie die de API en de UI allebei gebruiken |
| `API` | Controllers, JWT, middleware voor foutafhandeling en Swagger |
| `UI` | Blazor-pagina's en services die de API aanroepen |
| `Tests` | Controles op de bedrijfsregels met een tijdelijke SQLite-database |

## Gebruikte technieken
- C# / .NET 8, ASP.NET Core Web API, Blazor
- Entity Framework Core met SQLite en migrations
- JWT-authenticatie en autorisatie op rollen
- Middleware voor centrale foutafhandeling
- Swagger

## Starten
1. Maak de database aan en start de API:
   ```bash
   dotnet run --project API --launch-profile http -- --migrate
   ```
2. Start de UI in een tweede terminal:
   ```bash
   dotnet run --project UI --launch-profile http
   ```
3. Open http://localhost:5141.

**Tests**
```bash
dotnet run --project Tests
```

### Demo-account
| E-mail | Wachtwoord | Rol |
| --- | --- | --- |
| `test@actievadis.nl` | `Welkom123!` | Beheerder |

> De JWT-sleutel in `appsettings.Development.json` is alleen bedoeld om lokaal te testen.
