# innovia-hub4

## API-dokumentation (Scalar)

Projektet använder [Scalar](https://scalar.com/) för att visa och testa API:et interaktivt, baserat på OpenAPI-specen.

1. Starta API:t (`Innovia.Api`), t.ex. med `dotnet run` i mappen `Innovia.Api`.
2. Se till att du kör i Development-läge (`ASPNETCORE_ENVIRONMENT=Development`) — Scalar är bara aktiverat då.
3. Öppna webbläsaren på `/scalar/v1`, t.ex.:
   - `http://localhost:5123/scalar/v1`
   - `https://localhost:7229/scalar/v1`
4. I gränssnittet kan du bläddra bland alla endpoints, se request/response-scheman och skicka testanrop direkt.

