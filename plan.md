# Plan: Availability & Time Slots

Mål: frontend kan hämta lediga tidsslottar per resurs och få realtidsuppdatering när en slot bokas/avbokas. Regler för öppettider sätts per `ResourceType`, inte per enskild `Resource`. Slots beräknas on-the-fly (ingen lagring av genererade slots i DB).

## 1. Datamodell

### Ny entity: `AvailabilityRule` (`Common/Database/Entities/AvailabilityRule.cs`)

```csharp
public sealed class AvailabilityRule
{
    public Guid Id { get; set; }
    public Guid ResourceTypeId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpensAt { get; set; }      // lokal svensk väggklocka, t.ex. 08:00
    public TimeOnly ClosesAt { get; set; }     // t.ex. 16:00
    public int SlotDurationMinutes { get; set; } // t.ex. 60
}
```

- `OpensAt`/`ClosesAt` lagras som `TimeOnly` (ingen offset/datum) — de representerar svensk lokal tid, inte UTC. Omvandling till UTC sker per specifikt datum vid uträkning (se §3), så DST hanteras korrekt automatiskt istället för att frysas in i en lagrad offset.
- En `ResourceType` kan ha 0–7 rader (en per veckodag som är öppen). Ingen rad = stängt den dagen.
- FK: `AvailabilityRule.ResourceTypeId → ResourceType.Id`.

### EF Core

- `AppDbContext`: lägg till `DbSet<AvailabilityRule> AvailabilityRules`.
- `OnModelCreating`: `HasOne<ResourceType>().WithMany().HasForeignKey(x => x.ResourceTypeId)`, unik-index på `(ResourceTypeId, DayOfWeek)` (en regel per veckodag per resurstyp).
- Ny migration: `AddAvailabilityRules`.
- Seed: lägg till en `AvailabilityRuleSeeder` (samma mönster som `RoleSeeder`/`AdminUserSeeder` i `AuthenticationExtensions.cs`) som skapar standardregler (t.ex. mån–fre 08–16) för befintliga `ResourceType`-rader vid uppstart i dev, så feature går att testa direkt.

## 2. Feature-struktur

Bygg vidare på befintlig tomma mappen `Features/Availability/GetResourceAvailability` (vertical-slice-mönster, samma som `Bookings`/`Resources`):

```
Features/Availability/
  AvailabilityServiceExtensions.cs        (AddAvailabilityFeature + MapAvailabilityEndpoints)
  GetResourceAvailability/
    GetResourceAvailability.Command.cs    (ResourceId, FromDate, ToDate)
    GetResourceAvailability.Validator.cs  (FromDate <= ToDate, max intervall t.ex. 31 dagar)
    GetResourceAvailability.Handler.cs    (kärnlogik, se §3)
    GetResourceAvailability.Response.cs   (SlotDto[])
    GetResourceAvailability.Endpoint.cs   (GET /availability/resources/{resourceId}?from=&to=)
  ManageAvailabilityRules/                (admin-CRUD för regler, AdminOnly policy)
    ...Create/Update/Delete/List, samma mönster som Features/ResourceTypes
```

Auth: `GetResourceAvailability` → `MemberOrAdmin` (alla inloggade får se lediga tider). `ManageAvailabilityRules/*` → `AdminOnly`. Sätts centralt i `AvailabilityServiceExtensions.MapAvailabilityEndpoints`, samma mönster som redan etablerat i `BookingServiceExtensions`.

### Response-kontrakt till frontend

```json
{
  "resourceId": "...",
  "slots": [
    { "startUtc": "2026-09-07T06:00:00Z", "endUtc": "2026-09-07T07:00:00Z", "isAvailable": true },
    { "startUtc": "2026-09-07T07:00:00Z", "endUtc": "2026-09-07T08:00:00Z", "isAvailable": false }
  ]
}
```

Tider går alltid över wire som UTC (`DateTimeOffset`/ISO 8601 med `Z`). Frontend gör lokal presentation — servern uttrycker sig aldrig i "svensk tid" utåt, bara internt vid uträkning av öppettider.

## 3. Uträkningslogik (on-the-fly, ingen lagring av slots)

`GetResourceAvailability.Handler`:

1. Slå upp `Resource` → `ResourceTypeId`.
2. Hämta `AvailabilityRule`-raderna för den `ResourceTypeId`.
3. För varje datum i `[FromDate, ToDate]`:
   - Hitta regeln för det datumets `DayOfWeek`. Ingen regel → inga slots den dagen.
   - Bygg lokal `DateTime` för `date + OpensAt` och `date + ClosesAt` (`DateTimeKind.Unspecified`).
   - Konvertera till UTC via `TimeZoneInfo.ConvertTimeToUtc(localDateTime, SwedenTimeZone)` — se §4. Detta är den enda platsen tidszon-logik körs; DST-hopp (mars/oktober) hanteras korrekt per datum eftersom vi konverterar varje dag för sig, inte en gång globalt.
   - Dela upp `[opensUtc, closesUtc)` i `SlotDurationMinutes`-block.
4. Hämta alla `Booking` för `ResourceId` där `CancelledAt == null` och `StartsAt < ToDateUtc && EndsAt > FromDateUtc` (overlap-query, ett enda DB-anrop för hela intervallet).
5. Markera varje genererad slot `IsAvailable = false` om den överlappar någon hämtad bokning, annars `true`.
6. Ingen skrivning till DB — rent read-side, alltid konsistent med senaste bokningsläget.

Detta betyder också att om en `AvailabilityRule` ändras (admin ändrar öppettider) slår det igenom direkt utan migrering av data — inga förgenererade slot-rader att städa upp.

## 4. Tidszonshantering (svensk tid → UTC)

Ny helper: `Common/Time/SwedenTimeZone.cs`

```csharp
public static class SwedenTimeZone
{
    public static readonly TimeZoneInfo Instance =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public static DateTime ToUtc(DateOnly date, TimeOnly localTime) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(date.ToDateTime(localTime), DateTimeKind.Unspecified),
            Instance);
}
```

- `"Europe/Stockholm"` är IANA-id och funkar cross-platform (Windows-dev-maskin *och* Linux-container) på .NET 6+ så länge ICU är tillgängligt — se deploy-anmärkning nedan. Undvik `"W. Europe Standard Time"` (bara Windows-id).
- All uträkning i handlern jobbar mot denna helper — ingen annan stans i koden ska anropa `TimeZoneInfo` direkt, så vi har en enda källa att uppdatera om vi nånsin behöver stödja fler tidszoner.

### Deploy-fälla att lösa nu, inte i produktion

Om API:et körs i en Linux-container (särskilt `alpine`-baserade .NET-images) saknas ofta `tzdata`/ICU, vilket gör att `FindSystemTimeZoneById("Europe/Stockholm")` kastar `TimeZoneNotFoundException` i produktion även om allt funkar lokalt på dev-maskinen.

Åtgärd i Dockerfile (när den skapas):
- Använd `mcr.microsoft.com/dotnet/aspnet:10.0` (Debian-baserad) istället för `-alpine`-varianten, **eller**
- Om alpine krävs: `RUN apk add --no-cache icu-libs tzdata` och se till att `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` **inte** är satt till `true`.
- Lägg till ett litet startup-check (t.ex. i `Program.cs` innan `app.Run()`) som anropar `SwedenTimeZone.Instance` en gång och failar fast/loggar tydligt vid uppstart om tidszonen saknas — hellre krasch vid deploy än tyst fel uträkning i produktion.
- Testtäckning: enhetstest som kör `SwedenTimeZone.ToUtc` för datum precis runt DST-bytena (sista söndagen i mars/oktober) och verifierar att offset blir +02:00 respektive +01:00 som förväntat.

## 5. Realtidsuppdatering

Lägg till SignalR (finns inte i projektet ännu):

- Nuget: `Microsoft.AspNetCore.SignalR` (ingår i ASP.NET Core-sharen, inget extra paket behövs för servern).
- `Common/Realtime/AvailabilityHub.cs`: tom hub, klienter joinar grupp `$"resource:{resourceId}"` via `HubConnection.InvokeAsync("JoinResourceGroup", resourceId)`.
- `Program.cs`: `builder.Services.AddSignalR();` och `app.MapHub<AvailabilityHub>("/hubs/availability");`.
- Injicera `IHubContext<AvailabilityHub>` i `CreateBooking.Handler`, `CancelBooking.Handler` och `UpdateBooking.Handler`. Efter lyckad commit: skicka till gruppen `resource:{resourceId}`:
  ```json
  { "type": "SlotUpdated", "resourceId": "...", "startUtc": "...", "endUtc": "...", "isAvailable": false }
  ```
- Frontend behöver inte refetcha hela intervallet — patchar bara den enskilda sloten i sin lokala state. Vid reconnect (t.ex. efter tappad uppkoppling) gör frontend en vanlig `GET /availability/...` för att synka om, så SignalR behöver aldrig vara 100% leverans-garanterad.
- Auth på hubben: samma JWT-cookie/bearer som redan används (`.RequireAuthorization()` på hub-mappningen), ingen ny auth-mekanism.

## 6. Ordning att implementera i

1. `AvailabilityRule`-entity + migration + `AppDbContext`.
2. `SwedenTimeZone`-helper + DST-enhetstest.
3. `GetResourceAvailability` (handler, endpoint, `MemberOrAdmin`) — testbar och användbar helt utan realtidsdelen.
4. `ManageAvailabilityRules` CRUD (admin) så regler går att sätta utan att gå via databasen manuellt.
5. SignalR-hub + koppling in i Create/Update/Cancel-handlers.
6. Dockerfile + tzdata/ICU-fix + startup-check, verifierat i en container lokalt innan första deploy.
