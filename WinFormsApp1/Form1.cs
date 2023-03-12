namespace WinFormsApp1;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        Load += Form1_Load;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        Button helloButton = new();
        helloButton.BackColor = Color.LightGray;
        helloButton.ForeColor = Color.Black;
        helloButton.Location = new Point(10, 10);
        helloButton.Size = new Size(100, 50);
        helloButton.Text = "Hello!!!";
        Controls.Add(helloButton);
    }
}