using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

public class LoginForm : Form
{
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;
    private Button btnGuest;

    public LoginForm()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        AppStyle.ApplyForm(this);
        Text = "Вход — СтройМатериалы";
        ClientSize = new Size(420, 260);

        var lblTitle = new Label
        {
            Text = "СтройМатериалы",
            Font = AppStyle.TitleFont,
            ForeColor = AppStyle.PrimaryDark,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Left = 0,
            Top = 15,
            Width = 420,
            Height = 30
        };

        var lblLogin = new Label { Text = "Логин:", Left = 40, Top = 70, Width = 80 };
        var lblPassword = new Label { Text = "Пароль:", Left = 40, Top = 110, Width = 80 };

        txtLogin = new TextBox
        {
            Left = 130,
            Top = 67,
            Width = 240
        };

        txtPassword = new TextBox
        {
            Left = 130,
            Top = 107,
            Width = 240,
            UseSystemPasswordChar = true
        };

        btnLogin = new Button
        {
            Text = "Войти",
            Left = 130,
            Top = 155,
            Width = 115
        };
        AppStyle.ApplyButton(btnLogin);

        btnGuest = new Button
        {
            Text = "Гость",
            Left = 255,
            Top = 155,
            Width = 115
        };
        AppStyle.ApplyButton(btnGuest);
        btnGuest.BackColor = Color.Gray;

        Controls.Add(lblTitle);
        Controls.Add(lblLogin);
        Controls.Add(lblPassword);
        Controls.Add(txtLogin);
        Controls.Add(txtPassword);
        Controls.Add(btnLogin);
        Controls.Add(btnGuest);

        btnLogin.Click += BtnLoginClick;
        btnGuest.Click += BtnGuestClick;
        AcceptButton = btnLogin;
    }

    private void BtnLoginClick(object sender, EventArgs e)
    {
        using (var conn = Db.Open())
        using (var cmd = new NpgsqlCommand(
            @"SELECT u.user_id,
                     u.surname || ' ' || u.first_name ||
                       COALESCE(' ' || u.patronymic, '') AS fio,
                     r.role_name
              FROM users u
              JOIN roles r ON r.role_id = u.role_id
              WHERE u.login = @l AND u.password = @p", conn))
        {
            cmd.Parameters.AddWithValue("l", txtLogin.Text.Trim());
            cmd.Parameters.AddWithValue("p", txtPassword.Text);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    MessageBox.Show("Неверный логин или пароль");
                    return;
                }

                string fio = reader.GetString(1);
                string role = reader.GetString(2);

                OpenProducts(fio, role);
            }
        }
    }

    private void BtnGuestClick(object sender, EventArgs e)
    {
        OpenProducts("Гость", "Гость");
    }

    private void OpenProducts(string fio, string role)
    {
        var form = new ProductsForm(fio, role);
        form.FormClosed += (s, e) => Show();
        form.Show();
        Hide();
    }
}