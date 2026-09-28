using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

public class OrderEditForm : Form
{
    private NumericUpDown numNumber;
    private DateTimePicker dtOrder;
    private DateTimePicker dtDelivery;
    private ComboBox cmbPoint;
    private ComboBox cmbUser;
    private TextBox txtCode;
    private ComboBox cmbStatus;

    public OrderEditForm(int? id)
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        AppStyle.ApplyForm(this);
        Text = "Новый заказ";
        ClientSize = new Size(460, 300);

        Controls.Add(new Label { Text = "Номер:", Left = 15, Top = 18, Width = 140 });
        numNumber = new NumericUpDown
        {
            Left = 170,
            Top = 15,
            Width = 260,
            Maximum = 1000000
        };
        Controls.Add(numNumber);

        Controls.Add(new Label { Text = "Дата заказа:", Left = 15, Top = 48, Width = 140 });
        dtOrder = new DateTimePicker { Left = 170, Top = 45, Width = 260 };
        Controls.Add(dtOrder);

        Controls.Add(new Label { Text = "Дата доставки:", Left = 15, Top = 78, Width = 140 });
        dtDelivery = new DateTimePicker { Left = 170, Top = 75, Width = 260 };
        Controls.Add(dtDelivery);

        Controls.Add(new Label { Text = "Пункт выдачи:", Left = 15, Top = 108, Width = 140 });
        cmbPoint = new ComboBox
        {
            Left = 170,
            Top = 105,
            Width = 260,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cmbPoint);

        Controls.Add(new Label { Text = "Клиент:", Left = 15, Top = 138, Width = 140 });
        cmbUser = new ComboBox
        {
            Left = 170,
            Top = 135,
            Width = 260,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cmbUser);

        Controls.Add(new Label { Text = "Код получения:", Left = 15, Top = 168, Width = 140 });
        txtCode = new TextBox { Left = 170, Top = 165, Width = 260 };
        Controls.Add(txtCode);

        Controls.Add(new Label { Text = "Статус:", Left = 15, Top = 198, Width = 140 });
        cmbStatus = new ComboBox
        {
            Left = 170,
            Top = 195,
            Width = 260,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cmbStatus);

        var btnSave = new Button
        {
            Text = "Сохранить",
            Left = 170,
            Top = 240,
            Width = 130
        };
        AppStyle.ApplyButton(btnSave);
        btnSave.Click += BtnSaveClick;
        Controls.Add(btnSave);

        FillCombo(cmbPoint,
            "SELECT point_id, address FROM pickup_points ORDER BY address");
        FillCombo(cmbUser,
            @"SELECT u.user_id,
                     u.surname || ' ' || u.first_name ||
                        COALESCE(' ' || u.patronymic, '') AS fio
              FROM users u
              JOIN roles r ON r.role_id = u.role_id
              WHERE r.role_name = 'Авторизированный клиент'
              ORDER BY u.surname, u.first_name");
        FillCombo(cmbStatus,
            "SELECT status_id, status_name FROM order_statuses ORDER BY status_id");
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

    private void BtnSaveClick(object sender, EventArgs e)
    {
        try
        {
            using (var conn = Db.Open())
            using (var cmd = new NpgsqlCommand(
                @"INSERT INTO orders
                    (order_number, order_date, delivery_date,
                     point_id, user_id, receive_code, status_id)
                  VALUES
                    (@num, @od, @dd, @p, @u, @c, @s)", conn))
            {
                cmd.Parameters.AddWithValue("num", (int)numNumber.Value);
                cmd.Parameters.AddWithValue("od", dtOrder.Value.Date);
                cmd.Parameters.AddWithValue("dd", dtDelivery.Value.Date);
                cmd.Parameters.AddWithValue("p", cmbPoint.SelectedValue);
                cmd.Parameters.AddWithValue("u", cmbUser.SelectedValue);
                cmd.Parameters.AddWithValue("c", txtCode.Text.Trim());
                cmd.Parameters.AddWithValue("s", cmbStatus.SelectedValue);
                cmd.ExecuteNonQuery();
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