using System.Net.Http.Json;
const string serverUrl = "https://localhost:7066";
using var http = new HttpClient { BaseAddress = new Uri(serverUrl) };
Console.WriteLine("Lab06 klient je pripraveny. Doplnte U3 az U5 podla navodu.");
// U3: vytvorte a nacitajte knihu cez HTTP.
// U4: doplnte gRPC klienta a cestu k obrazku.
// U5: spracujte chyby a casovy limit.
