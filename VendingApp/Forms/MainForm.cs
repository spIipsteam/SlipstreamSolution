using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;  

namespace VendingApp.Forms;

public partial class MainForm : Form
{
    private Panel sidebar;
    private Panel contentPanel;
    private Label lblUserInfo;
    private UserDto currentUser;
    private Chart chartSales;

    public MainForm(UserDto user)
    {
        currentUser = user;
        InitializeComponent();
        SetupUI();
        LoadDashboard();
    }

    private void SetupUI()
    {
        this.Text = "Личный кабинет Франчайзера";
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 240, 240);

        // ========== БОКОВАЯ ПАНЕЛЬ ==========
        sidebar = new Panel
        {
            Width = 250,
            Dock = DockStyle.Left,
            BackColor = Color.FromArgb(52, 73, 94)
        };

        // Кнопки меню
        var btnHome = CreateSidebarButton("🏠 Главная", 10);
        btnHome.Click += (s, e) => LoadDashboard();

        var btnMonitor = CreateSidebarButton("📊 Монитор ТА", 60);
        btnMonitor.Click += (s, e) => OpenMonitorForm();

        var btnAdmin = CreateSidebarButton("⚙️ Администрирование ТА", 110);
        btnAdmin.Click += (s, e) => OpenVendingForm();

        var btnProfile = CreateSidebarButton("👤 Профиль", 550);
        btnProfile.Click += (s, e) => ShowProfile();

        var btnLogout = CreateSidebarButton("🚪 Выход", 600);
        btnLogout.Click += (s, e) => Logout();

        // Информация о пользователе
        lblUserInfo = new Label
        {
            Text = $"{currentUser.FullName}\n{currentUser.Role}",
            Location = new Point(10, 500),
            Size = new Size(230, 60),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(44, 62, 80),
            Font = new Font("Arial", 10),
            TextAlign = ContentAlignment.MiddleCenter
        };
        sidebar.Controls.Add(lblUserInfo);

        // ========== ОСНОВНАЯ ОБЛАСТЬ ==========
        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 240, 240),
            AutoScroll = true
        };

        Controls.Add(sidebar);
        Controls.Add(contentPanel);
    }

    private Button CreateSidebarButton(string text, int y)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(10, y),
            Size = new Size(230, 45),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 11),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 62, 80);
        sidebar.Controls.Add(btn);
        return btn;
    }

    private async void LoadDashboard()
    {
        contentPanel.Controls.Clear();

        using var client = new HttpClient();

        try
        {
            // Получаем статистику
            var statsResponse = await client.GetStringAsync("http://localhost:5000/api/vending/stats");
            var stats = JsonSerializer.Deserialize<NetworkStats>(statsResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Блок 1: Эффективность сети
            int workingPercent = stats.total > 0 ? (stats.working * 100 / stats.total) : 0;
            var efficiencyPanel = CreateCard("📈 Эффективность сети", $"{workingPercent}%\nавтоматов работает", 20, 20, 280, 120, Color.FromArgb(76, 175, 80));
            contentPanel.Controls.Add(efficiencyPanel);

            // Блок 2: Состояние сети
            var statusPanel = CreateCard("📊 Состояние сети", $"🟢 Работает: {stats.working}\n🔴 Не работает: {stats.total - stats.working}", 320, 20, 280, 120, Color.FromArgb(33, 150, 243));
            contentPanel.Controls.Add(statusPanel);

            // Блок 3: Сводка (продажи)
            var summaryPanel = CreateCard("💰 Сводка", $"Общая выручка:\n{stats.revenue:C2}", 620, 20, 280, 120, Color.FromArgb(255, 152, 0));
            contentPanel.Controls.Add(summaryPanel);

            // Блок 4: Динамика продаж (график)
            var salesResponse = await client.GetStringAsync("http://localhost:5000/api/sales/last10days");
            var sales = JsonSerializer.Deserialize<SaleDay[]>(salesResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var chartPanel = new Panel
            {
                Location = new Point(20, 160),
                Size = new Size(880, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            chartSales = new Chart { Dock = DockStyle.Fill };
            var chartArea = new ChartArea();
            chartArea.AxisX.Title = "Дата";
            chartArea.AxisY.Title = "Сумма продаж (руб)";
            chartSales.ChartAreas.Add(chartArea);

            var series = new Series
            {
                Name = "Продажи",
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(76, 175, 80),
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8
            };
            chartSales.Series.Add(series);

            if (sales != null)
            {
                foreach (var day in sales)
                {
                    series.Points.AddXY(day.Date, day.Amount);
                }
            }

            // Кнопки фильтрации графика
            var filterPanel = new Panel { Location = new Point(20, 470), Size = new Size(880, 40) };
            var btnFilterAmount = new Button
            {
                Text = "По сумме",
                Location = new Point(10, 5),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            var btnFilterQuantity = new Button
            {
                Text = "По количеству",
                Location = new Point(120, 5),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White
            };
            filterPanel.Controls.Add(btnFilterAmount);
            filterPanel.Controls.Add(btnFilterQuantity);

            chartPanel.Controls.Add(chartSales);
            contentPanel.Controls.Add(chartPanel);
            contentPanel.Controls.Add(filterPanel);

            // Блок 5: Новости
            var newsResponse = await client.GetStringAsync("http://localhost:5000/api/news");
            var news = JsonSerializer.Deserialize<NewsItem[]>(newsResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            int yOffset = 530;
            var newsLabel = new Label
            {
                Text = "📰 Новости франчайзера",
                Location = new Point(20, yOffset),
                Size = new Size(880, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            contentPanel.Controls.Add(newsLabel);
            yOffset += 35;

            if (news != null)
            {
                foreach (var item in news)
                {
                    var newsItem = new Label
                    {
                        Text = $"• {item.Title}: {item.Content} ({item.Date})",
                        Location = new Point(30, yOffset),
                        Size = new Size(860, 25),
                        ForeColor = Color.FromArgb(70, 70, 70),
                        Font = new Font("Arial", 10)
                    };
                    contentPanel.Controls.Add(newsItem);
                    yOffset += 28;
                }
            }
        }
        catch (Exception ex)
        {
            var errorLabel = new Label
            {
                Text = $"Ошибка загрузки данных: {ex.Message}\nУбедитесь что API запущен на http://localhost:5000",
                Location = new Point(20, 20),
                Size = new Size(800, 60),
                ForeColor = Color.Red,
                Font = new Font("Arial", 10)
            };
            contentPanel.Controls.Add(errorLabel);
        }
    }

    private Panel CreateCard(string title, string content, int x, int y, int w, int h, Color color)
    {
        var panel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var titleLabel = new Label
        {
            Text = title,
            Location = new Point(10, 5),
            Size = new Size(w - 20, 25),
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 70, 70)
        };

        var contentLabel = new Label
        {
            Text = content,
            Location = new Point(10, 35),
            Size = new Size(w - 20, 70),
            Font = new Font("Arial", 14),
            ForeColor = color,
            TextAlign = ContentAlignment.TopCenter
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(contentLabel);
        return panel;
    }

    private void OpenMonitorForm()
    {
        var monitorForm = new MonitorForm();
        monitorForm.ShowDialog();
    }

    private void OpenVendingForm()
    {
        var vendingForm = new VendingMachinesForm();
        vendingForm.ShowDialog();
    }

    private void ShowProfile()
    {
        MessageBox.Show($"ФИО: {currentUser.FullName}\nEmail: {currentUser.Email}\nРоль: {currentUser.Role}", "Профиль", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Logout()
    {
        var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }
    }

    // Вспомогательные классы
    private class NetworkStats
    {
        public int total { get; set; }
        public int working { get; set; }
        public decimal revenue { get; set; }
    }

    private class SaleDay
    {
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
    }

    private class NewsItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}