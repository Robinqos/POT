using System.Collections;
using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Services;

/// <summary>U5: Indexery, params, IEnumerable, lokálne funkcie, ref, generiká a n-tice.</summary>
public class Depot(string name, string codePrefix) : IEnumerable<Package>
{
    public string Name { get; init; } = name;
    public string CodePrefix { get; init; } = codePrefix;
    private readonly List<Package> _packages = [];
    public int Count => _packages.Count;

    // Pripravený indexer. Neplatný index vyvolá ArgumentOutOfRangeException z List.
    public Package this[int index]
    {
        get => _packages[index];
        set => _packages[index] = value;
    }

    // U5a: Prvý zhodný kód bez rozlišovania veľkosti písmen alebo null.
    public Package? this[string trackingNumber]
    {
        get
        {
            // TODO U5a: Reťazcový indexer
            return _packages.FirstOrDefault(p =>
                        string.Equals(p.TrackingNumber, trackingNumber,
                            StringComparison.OrdinalIgnoreCase));
        }
    }

    // Pripravené params: hromadné vloženie bez doménovej validácie.
    public void Add(params Package[] packages)
    {
        foreach (var package in packages)
            if (package is not null) _packages.Add(package);
    }

    // U5b: Prefix porovnávajte OrdinalIgnoreCase; dĺžka >= minCodeLength.
    // Platná hmotnosť: > 0.01 a <= 100 kg. Neplatný balík nevložte.
    // Predvolené a pomenované argumenty si vyskúšajte v Program.cs.
    public bool RegisterWithValidation(Package package, int minCodeLength = 6)
    {
        // TODO U5b: Lokálne funkcie
        bool IsPrefixValid(string code) =>
                code.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase)
                && code.Length >= minCodeLength;

        static bool IsWeightValid(double weight) =>
            weight is > 0.01 and <= 100.0;

        if (!IsPrefixValid(package.TrackingNumber) || !IsWeightValid(package.WeightKg))
            return false;

        _packages.Add(package);
        return true;
    }

    // Pripravené: Swap mení odovzdané premenné, samotné depo nemení.
    public static void Swap<T>(ref T a, ref T b)
    {
        T temporary = a;
        a = b;
        b = temporary;
    }

    // U5c: Pomenovaná n-tica; prázdne depo vracia (0, 0m, 0.0).
    public (int TotalCount, decimal TotalValue, double TotalWeightKg) GetStatistics()
    {
        // TODO U5c: Štatistiky depa
        return (
                _packages.Count,
                _packages.Sum(p => p.BasePrice),
                _packages.Sum(p => p.WeightKg)
            );
    }

    public IEnumerator<Package> GetEnumerator() => _packages.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
