using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

public class OrdersForm : Form
{
    private DataGridView grid;
    private string userRole;

    public OrdersForm(string role)
    {
        userRole = role;
        InitializeUi();
        LoadOrders();
    }

    private void InitializeUi()
    {
        AppStyle.ApplyForm(this);
        Text = "Заказы — СтройМатериалы";
        ClientSize = new Size(1000, 600);

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 45 };

        if (userRole == "Администратор")
        {
            var btnAdd = new Button
            {
                Text = "Добавить заказ",
                Left = 10,
                Top = 8,
                Width = 140
            };
            AppStyle.ApplyButton(btnAdd);
            btnAdd.Click += (s, e) =>
            {
                using (var f = new OrderEditForm(null))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                        LoadOrders();
                }
            };

            var btnDelete = new Button
            {
                Text = "Удалить заказ",
                Left = 160,
                Top = 8,
                Width = 140
            };
            AppStyle.ApplyButton(btnDelete);
            btnDelete.BackColor = Color.Firebrick;
            btnDelete.Click += BtnDeleteClick;

            topPanel.Controls.Add(btnAdd);
            topPanel.Controls.Add(btnDelete);
        }

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false
        };

        Controls.Add(grid);
        Controls.Add(topPanel);
    }

    private void LoadOrders()
    {
        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(
            @"SELECT o.order_id,
                     o.order_number,
                     o.order_date,
                     o.delivery_date,
                     pp.address,
                     u.surname || ' ' || u.first_name ||
                        COALESCE(' ' || u.patronymic, '') AS client_fio,
                     o.receive_code,
                     os.status_name
              FROM orders o
              JOIN pickup_points  pp ON pp.point_id  = o.point_id
              JOIN users           u ON u.user_id    = o.user_id
              JOIN order_statuses os ON os.status_id = o.status_id
              ORDER BY o.order_number", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var dt = new DataTable();
            dt.Load(reader);
            grid.DataSource = dt;
        }

        if (grid.Columns.Contains("order_id"))
            grid.Columns["order_id"].Visible = false;
    }

    private void BtnDeleteClick(object sender, EventArgs e)
    {
        if (grid.CurrentRow == null)
            return;

        if (MessageBox.Show("Удалить заказ?", "Подтверждение",
            MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        int oid = (int)grid.CurrentRow.Cells["order_id"].Value;

        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(
            "DELETE FROM orders WHERE order_id = @id", conn))
        {
            cmd.Parameters.AddWithValue("id", oid);
            cmd.ExecuteNonQuery();
        }

        LoadOrders();
    }
}