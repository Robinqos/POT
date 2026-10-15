namespace KST.POT.AsyncExamples.U07aWinFormsSyncContext;

public partial class Form1 : Form
{
    public Form1()
    {
        this.InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        Console.WriteLine("SynchronizationContext.Current = " + (SynchronizationContext.Current?.ToString() ?? "(null)"));
    }
}