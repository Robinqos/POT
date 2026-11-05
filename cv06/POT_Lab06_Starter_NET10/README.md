# Lab06 cisty starter

Jedno riesenie, tri projekty, dva beziace procesy. Server obsluhuje REST aj gRPC na jednej HTTPS adrese. Client ich vola. GrpcContracts je spolocna kniznica, nespusta sa samostatne; doplnate ju az v U4. Kazdy csproj ma vlastne nastavenia net10.0, Nullable a ImplicitUsings. Directory.Build.props sa nepouziva.

Otvorte Lab06.slnx vo Visual Studio 2026 s .NET 10 SDK.
Spustite `dotnet build Lab06.slnx`, potom `dotnet run --project Lab06.Server --launch-profile https`.
Server: https://localhost:7066/swagger. Klient: `dotnet run --project Lab06.Client`.
Pri potrebe dovery certifikatu pouzite `dotnet dev-certs https --trust`.

Projekty, referencie, OpenAPI a Swagger UI su pripravene. Knihy, validacia, HTTP klient a gRPC nie su vyriesene.
Balicky pre gRPC a ImageSharp doplnte pri U4 podla NAVOD.md. Povodne pomocne subory su v Podklady; BookHelper.cs najskor opravte podla U1.
Starter nema dodane automaticke testy. Overujte cez pokusy v navode.


## VS Code
Otvorte koreň tohto priečinka cez File > Open Folder. Nainštalujte odporúčané rozšírenie C# Dev Kit. Projekty už existujú, nové nevytvárajte. Podrobný postup, príkazy aj AI prompty sú v NAVOD.md. Ctrl+Shift+B spustí pripravenú úlohu Lab06 build.

Terminál 1: `dotnet run --project Lab06.Server --launch-profile https`

Terminál 2: `dotnet run --project Lab06.Client`

Čistý klient ešte nevolá API. Voliteľný Teplomer je zadanie na doplnenie po U4, nie implementované riešenie.
