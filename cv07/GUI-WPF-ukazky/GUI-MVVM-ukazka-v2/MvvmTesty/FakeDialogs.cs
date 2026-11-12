using MvvmSpolocne;

namespace MvvmTesty;

// slajd 19 – v teste sa namiesto skutočných dialógov podstrčí falošná implementácia
class FakeDialogs : IDialogService
{
    public bool Odpoved { get; set; } = true;          // „používateľ“ klikne Áno / Nie
    public string? Posledna { get; private set; }      // posledná zobrazená správa
    public int PocetOtazok { get; private set; }

    public void ShowMessage(string text, string title = "") => Posledna = text;

    public bool Confirm(string text, string title = "")
    {
        PocetOtazok++;
        return Odpoved;
    }
}
