using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

public class ProductEditForm : Form
{
    private int? productId;

    private TextBox txtArticle;
    private TextBox txtName;
    private TextBox txtUnit;
    private TextBox txtPrice;
    private TextBox txtDiscount;
    private TextBox txtStock;
    private TextBox txtDescription;
    private ComboBox cmbSupplier;
    private ComboBox cmbManufacturer;
    private ComboBox cmbCategory;

    public ProductEditForm(int? id)
    {
        productId = id;
        InitializeUi();
        LoadReferences();
        if (id != null)
            LoadProduct();
    }

    private void InitializeUi()
    {
        AppStyle.ApplyForm(this);
        Text = productId == null ? "Новый товар" : "Редактирование товара";
        ClientSize = new Size(440, 430);

        AddLabel("Артикул:", 15);
        txtArticle = AddTextBox(15);

        AddLabel("Наименование:", 50);
        txtName = AddTextBox(50);

        AddLabel("Ед. изм.:", 85);
        txtUnit = AddTextBox(85);

        AddLabel("Цена:", 120);
        txtPrice = AddTextBox(120);

        AddLabel("Скидка (%):", 155);
        txtDiscount = AddTextBox(155);

        AddLabel("Кол-во на складе:", 190);
        txtStock = AddTextBox(190);

        AddLabel("Описание:", 225);
        txtDescription = new TextBox
        {
            Left = 160,
            Top = 225,
            Width = 260,
            Height = 50,
            Multiline = true
        };
        Controls.Add(txtDescription);

        AddLabel("Поставщик:", 285);
        cmbSupplier = AddCombo(285);

        AddLabel("Производитель:", 315);
        cmbManufacturer = AddCombo(315);

        AddLabel("Категория:", 345);
        cmbCategory = AddCombo(345);

        var btnSave = new Button
        {
            Text = "Сохранить",
            Left = 160,
            Top = 385,
            Width = 130
        };
        AppStyle.ApplyButton(btnSave);
        btnSave.Click += BtnSaveClick;
        Controls.Add(btnSave);
    }

    private void AddLabel(string text, int top)
    {
        Controls.Add(new Label { Text = text, Left = 15, Top = top + 3, Width = 140 });
    }

    private TextBox AddTextBox(int top)
    {
        var tb = new TextBox { Left = 160, Top = top, Width = 260 };
        Controls.Add(tb);
        return tb;
    }

    private ComboBox AddCombo(int top)
    {
        var cb = new ComboBox
        {
            Left = 160,
            Top = top,
            Width = 260,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cb);
        return cb;
    }

    private void LoadReferences()
    {
        FillCombo(cmbSupplier, "SELECT supplier_id, supplier_name FROM suppliers ORDER BY supplier_name");
        FillCombo(cmbManufacturer, "SELECT manufacturer_id, manufacturer_name FROM manufacturers ORDER BY manufacturer_name");
        FillCombo(cmbCategory, "SELECT category_id, category_name FROM categories ORDER BY category_name");
    }

    private void FillCombo(ComboBox combo, string sql)
    {
        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(sql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            var dt = new DataTable();
            dt.Load(reader);
            combo.DataSource = dt;
            combo.DisplayMember = dt.Columns[1].ColumnName;
            combo.ValueMember = dt.Columns[0].ColumnName;
        }
    }

    private void LoadProduct()
    {
        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(
            "SELECT * FROM products WHERE product_id = @id", conn))
        {
            cmd.Parameters.AddWithValue("id", productId.Value);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return;

                txtArticle.Text = reader["article"].ToString();
                txtName.Text = reader["name"].ToString();
                txtUnit.Text = reader["unit"].ToString();
                txtPrice.Text = reader["price"].ToString();
                txtDiscount.Text = reader["discount"].ToString();
                txtStock.Text = reader["stock_quantity"].ToString();
                txtDescription.Text = reader["description"].ToString();

                cmbSupplier.SelectedValue = reader["supplier_id"];
                cmbManufacturer.SelectedValue = reader["manufacturer_id"];
                cmbCategory.SelectedValue = reader["category_id"];
            }
        }
    }

    private void BtnSaveClick(object sender, EventArgs e)
    {
        try
        {
            using (var conn = Db.Open())
            {
                string sql;
                if (productId == null)
                {
                    sql =
                        @"INSERT INTO products
                            (article, name, unit, price, supplier_id, manufacturer_id,
                             category_id, discount, stock_quantity, description)
                          VALUES
                            (@a, @n, @u, @p, @s, @m, @c, @d, @q, @desc)";
                }
                else
                {
                    sql =
                        @"UPDATE products
                          SET article = @a, name = @n, unit = @u, price = @p,
                              supplier_id = @s, manufacturer_id = @m,
                              category_id = @c, discount = @d,
                              stock_quantity = @q, description = @desc
                          WHERE product_id = @id";
                }

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("a", txtArticle.Text.Trim());
                    cmd.Parameters.AddWithValue("n", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("u", txtUnit.Text.Trim());
                    cmd.Parameters.AddWithValue("p", decimal.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("s", cmbSupplier.SelectedValue);
                    cmd.Parameters.AddWithValue("m", cmbManufacturer.SelectedValue);
                    cmd.Parameters.AddWithValue("c", cmbCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("d", int.Parse(txtDiscount.Text));
                    cmd.Parameters.AddWithValue("q", int.Parse(txtStock.Text));
                    cmd.Parameters.AddWithValue("desc", txtDescription.Text);

                    if (productId != null)
                        cmd.Parameters.AddWithValue("id", productId.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}