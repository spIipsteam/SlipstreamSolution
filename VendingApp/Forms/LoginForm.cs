using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace VendingApp.Forms;

public partial class LoginForm : Form
{
    private TextBox txtEmail;
    private TextBox txtPassword;
    private Button btnLogin;
    private Label lblError;

    public LoginForm()
    {
        InitializeComponent();
        SetupForm();
    }

    private void SetupForm()
    {
        this.Text = "Авторизация - Вендинговая сеть";
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.BackColor = Color.White;

        var lblTitle = new Label
        {
            Text = "Вход в личный кабинет",
            Location = new Point(50, 20),
            Size = new Size(300, 30),
            Font = new Font("Arial", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 73, 94),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblEmail = new Label
        {
            Text = "Email:",
            Location = new Point(50, 70),
            Size = new Size(80, 25),
            Font = new Font("Arial", 10)
        };

        txtEmail = new TextBox
        {
            Location = new Point(140, 70),
            Size = new Size(180, 25),
            Font = new Font("Arial", 10)
        };

        var lblPassword = new Label
        {
            Text = "Пароль:",
            Location = new Point(50, 110),
            Size = new Size(80, 25),
            Font = new Font("Arial", 10)
        };

        txtPassword = new TextBox
        {
            Location = new Point(140, 110),
            Size = new Size(180, 25),
            Font = new Font("Arial", 10),
            PasswordChar = '*'
        };

        btnLogin = new Button
        {
            Text = "Войти",
            Location = new Point(140, 160),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            Font = new Font("Arial", 10),
            FlatStyle = FlatStyle.Flat
        };
        btnLogin.Click += BtnLogin_Click;

        lblError = new Label
        {
            Location = new Point(50, 210),
            Size = new Size(300, 30),
            ForeColor = Color.Red,
            Text = "",
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(lblTitle);
        Controls.Add(lblEmail);
        Controls.Add(txtEmail);
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(btnLogin);
        Controls.Add(lblError);
    }

    private async void BtnLogin_Click(object sender, EventArgs e)
    {
        btnLogin.Enabled = false;
        lblError.Text = "";
        btnLogin.Text = "Вход...";

        try
        {
            using var client = new HttpClient();
            var loginData = new { Email = txtEmail.Text, Password = txtPassword.Text };
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5000/api/auth/login", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var user = JsonSerializer.Deserialize<MainForm.UserDto>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                MessageBox.Show($"Добро пожаловать, {user.FullName}!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var mainForm = new MainForm(user);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                lblError.Text = "Неверный email или пароль";
            }
        }
        catch (Exception ex)
        {
            lblError.Text = $"Ошибка: {ex.Message}";
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text = "Войти";
        }
    }
}