using System.Drawing;
using System.Windows.Forms;

public static class AppStyle
{
    public static readonly Font BaseFont = new Font("Segoe UI", 9F);
    public static readonly Font HeaderFont = new Font("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font TitleFont = new Font("Segoe UI", 12F, FontStyle.Bold);

    public static readonly Color Primary = Color.FromArgb(70, 130, 180);
    public static readonly Color PrimaryDark = Color.FromArgb(45, 95, 135);
    public static readonly Color Background = Color.WhiteSmoke;
    public static readonly Color DiscountRow = Color.FromArgb(0xF4, 0xA4, 0x60);
    public static readonly Color OutOfStock = Color.LightBlue;
    public static readonly Color OldPrice = Color.Red;
    public static readonly Color NewPrice = Color.Black;

    public static void ApplyForm(Form f)
    {
        f.Font = BaseFont;
        f.BackColor = Background;
        f.StartPosition = FormStartPosition.CenterScreen;
        f.FormBorderStyle = FormBorderStyle.FixedSingle;
        f.MaximizeBox = false;
    }

    public static void ApplyButton(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = Primary;
        b.ForeColor = Color.White;
        b.Font = HeaderFont;
        b.Height = 30;
        b.Cursor = Cursors.Hand;
    }
}