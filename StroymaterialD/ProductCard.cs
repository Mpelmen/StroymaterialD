using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class ProductCard : UserControl
{
    public int ProductId { get; set; }
    public string Article { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal Price { get; set; }
    public string Supplier { get; set; }
    public string Manufacturer { get; set; }
    public string Category { get; set; }
    public int Discount { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }
    public string Photo { get; set; }

    public event EventHandler<int> EditRequested;
    public event EventHandler<int> DeleteRequested;

    public ProductCard()
    {
        Width = 1050;
        Height = 160;
        Margin = new Padding(6);
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = Color.White;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        BuildUi();
    }

    private void BuildUi()
    {
        // ---- Подсветка строки ----
        if (Stock == 0)
            BackColor = AppStyle.OutOfStock;
        else if (Discount > 12)
            BackColor = AppStyle.DiscountRow;

        // ---- Фото ----
        var pic = new PictureBox
        {
            Left = 10,
            Top = 10,
            Width = 140,
            Height = 140,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };
        pic.Image = LoadPhotoOrDefault();
        Controls.Add(pic);

        // ---- Заголовок: Категория | Наименование ----
        var lblTitle = new Label
        {
            Text = Category + " | " + Name,
            Font = AppStyle.HeaderFont,
            Left = 165,
            Top = 10,
            Width = 640,
            Height = 22,
            AutoEllipsis = true
        };
        Controls.Add(lblTitle);

        // ---- Описание, производитель, поставщик, ед., кол-во ----
        var lblDesc = new Label
        {
            Text = "Описание товара: " + Description,
            Left = 165,
            Top = 36,
            Width = 640,
            Height = 32,
            AutoEllipsis = true
        };
        var lblMan = new Label
        {
            Text = "Производитель: " + Manufacturer,
            Left = 165,
            Top = 70,
            Width = 640,
            Height = 18
        };
        var lblSup = new Label
        {
            Text = "Поставщик: " + Supplier,
            Left = 165,
            Top = 90,
            Width = 640,
            Height = 18
        };
        var lblUnit = new Label
        {
            Text = "Единица измерения: " + Unit,
            Left = 165,
            Top = 110,
            Width = 640,
            Height = 18
        };
        var lblStock = new Label
        {
            Text = "Количество на складе: " + Stock,
            Left = 165,
            Top = 130,
            Width = 640,
            Height = 18
        };

        Controls.Add(lblDesc);
        Controls.Add(lblMan);
        Controls.Add(lblSup);
        Controls.Add(lblUnit);
        Controls.Add(lblStock);

        // ---- Блок цены ----
        var lblPriceCaption = new Label
        {
            Text = "Цена:",
            Left = 815,
            Top = 40,
            Width = 60,
            Height = 18
        };
        Controls.Add(lblPriceCaption);

        if (Discount > 0)
        {
            decimal finalPrice = Price - (Price * Discount / 100m);

            var lblOldPrice = new Label
            {
                Text = Price.ToString("0.00") + " ₽",
                ForeColor = AppStyle.OldPrice,
                Font = new Font("Segoe UI", 10F, FontStyle.Strikeout),
                Left = 815,
                Top = 60,
                Width = 120,
                Height = 20
            };
            var lblNewPrice = new Label
            {
                Text = finalPrice.ToString("0.00") + " ₽",
                ForeColor = AppStyle.NewPrice,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Left = 815,
                Top = 82,
                Width = 120,
                Height = 22
            };

            Controls.Add(lblOldPrice);
            Controls.Add(lblNewPrice);
        }
        else
        {
            var lblPrice = new Label
            {
                Text = Price.ToString("0.00") + " ₽",
                ForeColor = AppStyle.NewPrice,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Left = 815,
                Top = 62,
                Width = 120,
                Height = 22
            };
            Controls.Add(lblPrice);
        }

        // ---- Блок скидки ----
        var lblDiscountCaption = new Label
        {
            Text = "Действующая",
            Left = 950,
            Top = 50,
            Width = 90,
            Height = 16,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var lblDiscountCaption2 = new Label
        {
            Text = "скидка",
            Left = 950,
            Top = 66,
            Width = 90,
            Height = 16,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var lblDiscountValue = new Label
        {
            Text = Discount + " %",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = AppStyle.PrimaryDark,
            Left = 950,
            Top = 88,
            Width = 90,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(lblDiscountCaption);
        Controls.Add(lblDiscountCaption2);
        Controls.Add(lblDiscountValue);

        // ---- Кнопки для администратора (появятся снаружи через события) ----
        var btnEdit = new Button
        {
            Text = "Изменить",
            Left = 950,
            Top = 122,
            Width = 90,
            Height = 24,
            Visible = false
        };
        var btnDelete = new Button
        {
            Text = "Удалить",
            Left = 950,
            Top = 148,
            Width = 90,
            Height = 24,
            Visible = false
        };

        btnEdit.Click += (s, e) => EditRequested?.Invoke(this, ProductId);
        btnDelete.Click += (s, e) => DeleteRequested?.Invoke(this, ProductId);

        Controls.Add(btnEdit);
        Controls.Add(btnDelete);
    }

    private Image LoadPhotoOrDefault()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(Photo))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Resources", "Photo", Photo);
                if (File.Exists(path))
                    return Image.FromFile(path);
            }
        }
        catch
        {
            // ignore, вернём заглушку
        }

        string defaultPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Resources", "picture.png");

        if (File.Exists(defaultPath))
            return Image.FromFile(defaultPath);

        return null;
    }
}