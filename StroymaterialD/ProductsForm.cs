using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

public class ProductsForm : Form
{
    private string userName;
    private string userRole;

    private FlowLayoutPanel cardsPanel;
    private TextBox txtSearch;
    private ComboBox cmbSort;
    private ComboBox cmbFilter;
    private Label lblUser;

    public ProductsForm(string fio, string role)
    {
        userName = fio;
        userRole = role;
        InitializeUi();
        LoadCategories();
        LoadProducts();
    }

    private void InitializeUi()
    {
        AppStyle.ApplyForm(this);
        Text = "Товары — СтройМатериалы";
        ClientSize = new Size(1100, 720);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;

        // ---- Верхняя панель ----
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppStyle.Primary
        };

        var lblLogo = new Label
        {
            Text = "СтройМатериалы",
            ForeColor = Color.White,
            Font = AppStyle.TitleFont,
            Left = 15,
            Top = 15,
            Width = 250,
            Height = 30
        };

        lblUser = new Label
        {
            Text = userName,
            ForeColor = Color.White,
            Font = AppStyle.HeaderFont,
            TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        lblUser.Left = ClientSize.Width - 320;
        lblUser.Top = 15;
        lblUser.Width = 220;
        lblUser.Height = 30;

        var btnLogout = new Button
        {
            Text = "Выход",
            Width = 70,
            Height = 30,
            Top = 15,
            Left = ClientSize.Width - 90,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        AppStyle.ApplyButton(btnLogout);
        btnLogout.BackColor = Color.Firebrick;
        btnLogout.Click += (s, e) => Close();

        topPanel.Controls.Add(lblLogo);
        topPanel.Controls.Add(lblUser);
        topPanel.Controls.Add(btnLogout);

        // ---- Панель фильтров ----
        var filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White
        };

        filterPanel.Controls.Add(new Label { Text = "Поиск:", Left = 10, Top = 17, Width = 55 });
        txtSearch = new TextBox { Left = 70, Top = 14, Width = 220 };
        filterPanel.Controls.Add(txtSearch);

        cmbSort = new ComboBox
        {
            Left = 310,
            Top = 14,
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbSort.Items.AddRange(new object[]
        {
            "Без сортировки", "Цена ↑", "Цена ↓", "Наименование"
        });
        cmbSort.SelectedIndex = 0;
        filterPanel.Controls.Add(cmbSort);

        cmbFilter = new ComboBox
        {
            Left = 510,
            Top = 14,
            Width = 240,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        filterPanel.Controls.Add(cmbFilter);

        // ---- Панель карточек ----
        cardsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10),
            BackColor = AppStyle.Background
        };

        Controls.Add(cardsPanel);
        Controls.Add(filterPanel);
        Controls.Add(topPanel);

        // ---- Права доступа ----
        bool isManager = userRole == "Менеджер";
        bool isAdmin = userRole == "Администратор";
        bool canFilter = isManager || isAdmin;

        if (!canFilter)
        {
            txtSearch.Visible = false;
            cmbSort.Visible = false;
            cmbFilter.Visible = false;
        }
        else
        {
            txtSearch.TextChanged += (s, e) => LoadProducts();
            cmbSort.SelectedIndexChanged += (s, e) => LoadProducts();
            cmbFilter.SelectedIndexChanged += (s, e) => LoadProducts();
        }

        if (isManager || isAdmin)
        {
            var btnOrders = new Button
            {
                Text = "Заказы",
                Left = 780,
                Top = 12,
                Width = 100,
                Height = 28
            };
            AppStyle.ApplyButton(btnOrders);
            btnOrders.Click += (s, e) => new OrdersForm(userRole).Show();
            filterPanel.Controls.Add(btnOrders);
        }

        if (isAdmin)
        {
            var btnAdd = new Button
            {
                Text = "Добавить",
                Left = 890,
                Top = 12,
                Width = 100,
                Height = 28
            };
            AppStyle.ApplyButton(btnAdd);
            btnAdd.BackColor = Color.SeaGreen;
            btnAdd.Click += (s, e) =>
            {
                using (var f = new ProductEditForm(null))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                        LoadProducts();
                }
            };
            filterPanel.Controls.Add(btnAdd);
        }
    }

    private void LoadCategories()
    {
        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(
            "SELECT category_name FROM categories ORDER BY category_name", conn))
        using (var reader = cmd.ExecuteReader())
        {
            cmbFilter.Items.Add("Все категории");
            while (reader.Read())
                cmbFilter.Items.Add(reader.GetString(0));
        }
        cmbFilter.SelectedIndex = 0;
    }

    private void LoadProducts()
    {
        cardsPanel.SuspendLayout();
        cardsPanel.Controls.Clear();

        using (var conn = Db.Open())
        {
            string sql =
                @"SELECT p.product_id, p.article, p.name, p.unit, p.price,
                         s.supplier_name, m.manufacturer_name, c.category_name,
                         p.discount, p.stock_quantity, p.description, p.photo
                  FROM products p
                  JOIN suppliers     s ON s.supplier_id     = p.supplier_id
                  JOIN manufacturers m ON m.manufacturer_id = p.manufacturer_id
                  JOIN categories    c ON c.category_id     = p.category_id
                  WHERE 1 = 1";

            if (txtSearch.Visible && !string.IsNullOrWhiteSpace(txtSearch.Text))
                sql += " AND (p.name ILIKE @q OR p.article ILIKE @q)";

            if (cmbFilter.Visible && cmbFilter.SelectedIndex > 0)
                sql += " AND c.category_name = @cat";

            if (cmbSort.Visible)
            {
                switch (cmbSort.SelectedIndex)
                {
                    case 1: sql += " ORDER BY p.price ASC"; break;
                    case 2: sql += " ORDER BY p.price DESC"; break;
                    case 3: sql += " ORDER BY p.name"; break;
                    default: sql += " ORDER BY p.product_id"; break;
                }
            }
            else
                sql += " ORDER BY p.product_id";

            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                if (txtSearch.Visible && !string.IsNullOrWhiteSpace(txtSearch.Text))
                    cmd.Parameters.AddWithValue("q", "%" + txtSearch.Text.Trim() + "%");
                if (cmbFilter.Visible && cmbFilter.SelectedIndex > 0)
                    cmd.Parameters.AddWithValue("cat", cmbFilter.SelectedItem.ToString());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new ProductCard
                        {
                            ProductId = reader.GetInt32(0),
                            Article = reader.GetString(1),
                            Name = reader.GetString(2),
                            Unit = reader.GetString(3),
                            Price = reader.GetDecimal(4),
                            Supplier = reader.GetString(5),
                            Manufacturer = reader.GetString(6),
                            Category = reader.GetString(7),
                            Discount = reader.GetInt32(8),
                            Stock = reader.GetInt32(9),
                            Description = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Photo = reader.IsDBNull(11) ? null : reader.GetString(11)
                        };

                        card.EditRequested += OnCardEditRequested;
                        card.DeleteRequested += OnCardDeleteRequested;

                        cardsPanel.Controls.Add(card);
                    }
                }
            }
        }

        cardsPanel.ResumeLayout();
    }

    private void OnCardEditRequested(object sender, int productId)
    {
        if (userRole != "Администратор")
            return;

        using (var f = new ProductEditForm(productId))
        {
            if (f.ShowDialog() == DialogResult.OK)
                LoadProducts();
        }
    }

    private void OnCardDeleteRequested(object sender, int productId)
    {
        if (userRole != "Администратор")
            return;

        if (MessageBox.Show("Удалить товар?", "Подтверждение",
            MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        try
        {
            using (var conn = Db.Open())
            using (var cmd = new NpgsqlCommand(
                "DELETE FROM products WHERE product_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("id", productId);
                cmd.ExecuteNonQuery();
            }
            LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}