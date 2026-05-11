using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace VendingApp.Forms;

public partial class MonitorForm : Form
{
    private DataGridView dgvMachines;
    private ComboBox cmbStatus;
    private ComboBox cmbConnection;
    private ComboBox cmbExtraStatus;
    private Button btnApply;
    private Button btnReset;
    private Label lblTotalCount;
    private Label lblTotalMoney;
    private Label lblFilterResult;
    private Panel filterPanel;

    // Данные автоматов (эмулируем динамические данные)
    private List<MachineMonitor> allMachines;
    private List<MachineMonitor> filteredMachines;

    public MonitorForm()
    {
        InitializeComponent();
        SetupUI();
        GenerateMockData();
        LoadMachines();
    }

    private void SetupUI()
    {
        this.Text = "Монитор ТА - Вендинговые автоматы";
        this.Size = new Size(1300, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 240, 240);

        // ========== ПАНЕЛЬ ФИЛЬТРОВ ==========
        filterPanel = new Panel
        {
            Location = new Point(10, 10),
            Size = new Size(1260, 80),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Фильтр по состоянию (зеленый/красный/синий)
        var lblStatus = new Label
        {
            Text = "Состояние:",
            Location = new Point(10, 15),
            Size = new Size(70, 25),
            Font = new Font("Arial", 10)
        };

        cmbStatus = new ComboBox
        {
            Location = new Point(85, 12),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Arial", 10)
        };
        cmbStatus.Items.AddRange(new string[] { "Все", "🟢 Работает", "🔴 Не работает", "🔵 На обслуживании" });
        cmbStatus.SelectedIndex = 0;

        // Фильтр по типу подключения
        var lblConnection = new Label
        {
            Text = "Подключение:",
            Location = new Point(260, 15),
            Size = new Size(80, 25),
            Font = new Font("Arial", 10)
        };

        cmbConnection = new ComboBox
        {
            Location = new Point(345, 12),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Arial", 10)
        };
        cmbConnection.Items.AddRange(new string[] { "Все", "4G", "WiFi", "Ethernet" });
        cmbConnection.SelectedIndex = 0;

        // Фильтр по дополнительным статусам
        var lblExtra = new Label
        {
            Text = "Доп. статус:",
            Location = new Point(520, 15),
            Size = new Size(80, 25),
            Font = new Font("Arial", 10)
        };

        cmbExtraStatus = new ComboBox
        {
            Location = new Point(605, 12),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Arial", 10)
        };
        cmbExtraStatus.Items.AddRange(new string[] { "Все", "Требует пополнения", "Низкий запас", "Норма" });
        cmbExtraStatus.SelectedIndex = 0;

        // Кнопка Применить
        btnApply = new Button
        {
            Text = "Применить",
            Location = new Point(780, 10),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnApply.Click += BtnApply_Click;

        // Кнопка Сбросить
        btnReset = new Button
        {
            Text = "Сбросить",
            Location = new Point(890, 10),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnReset.Click += BtnReset_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblStatus, cmbStatus,
            lblConnection, cmbConnection,
            lblExtra, cmbExtraStatus,
            btnApply, btnReset
        });

        // ========== ИНФОРМАЦИОННАЯ ПАНЕЛЬ ==========
        var infoPanel = new Panel
        {
            Location = new Point(10, 100),
            Size = new Size(1260, 50),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblTotalCount = new Label
        {
            Text = "Итого автоматов: 0",
            Location = new Point(10, 12),
            Size = new Size(250, 30),
            Font = new Font("Arial", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 73, 94)
        };

        lblTotalMoney = new Label
        {
            Text = "Денег в автоматах: 0 руб.",
            Location = new Point(350, 12),
            Size = new Size(300, 30),
            Font = new Font("Arial", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(76, 175, 80)
        };

        lblFilterResult = new Label
        {
            Text = "",
            Location = new Point(750, 12),
            Size = new Size(500, 30),
            Font = new Font("Arial", 10),
            ForeColor = Color.Gray
        };

        infoPanel.Controls.AddRange(new Control[] { lblTotalCount, lblTotalMoney, lblFilterResult });

        // ========== ТАБЛИЦА ==========
        dgvMachines = new DataGridView
        {
            Location = new Point(10, 160),
            Size = new Size(1260, 480),
            AllowUserToAddRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 248, 248) }
        };

        Controls.Add(filterPanel);
        Controls.Add(infoPanel);
        Controls.Add(dgvMachines);
    }

    private void GenerateMockData()
    {
        allMachines = new List<MachineMonitor>();

        var random = new Random();
        string[] statuses = { "Работает", "Не работает", "На обслуживании" };
        string[] connections = { "4G", "4G", "WiFi", "Ethernet" };
        string[] extraStatuses = { "Норма", "Требует пополнения", "Низкий запас", "Норма", "Норма" };
        string[] locations = { "ТЦ 'Европа'", "Бизнес-центр 'Плаза'", "Метро 'Центральное'", "Вокзал", "Университет" };

        for (int i = 1; i <= 15; i++)
        {
            var status = statuses[random.Next(statuses.Length)];
            var connection = connections[random.Next(connections.Length)];
            var extraStatus = extraStatuses[random.Next(extraStatuses.Length)];
            var location = locations[random.Next(locations.Length)];

            // Эмулируем денежные средства (случайно)
            decimal cashAmount = random.Next(1000, 50000);

            // Эмулируем загрузку
            int loadPercent = random.Next(30, 100);

            allMachines.Add(new MachineMonitor
            {
                Number = i,
                Name = $"ТА-{1000 + i}",
                Connection = connection,
                LoadPercent = loadPercent,
                CashAmount = cashAmount,
                Events = random.Next(0, 5),
                Equipment = $"Модель-{random.Next(100, 999)}",
                Info = location,
                Extra = extraStatus,
                Status = status,
                HardwareStatus = status == "Работает" ? "🟢" : (status == "Не работает" ? "🔴" : "🔵")
            });
        }
    }

    private void LoadMachines()
    {
        filteredMachines = new List<MachineMonitor>(allMachines);
        UpdateTable();
    }

    private void BtnApply_Click(object sender, EventArgs e)
    {
        filteredMachines = new List<MachineMonitor>(allMachines);

        // Фильтр по состоянию
        string statusFilter = cmbStatus.SelectedItem.ToString();
        if (statusFilter != "Все")
        {
            string status = statusFilter.Replace("🟢 ", "").Replace("🔴 ", "").Replace("🔵 ", "");
            filteredMachines = filteredMachines.FindAll(m => m.Status == status);
        }

        // Фильтр по типу подключения
        string connectionFilter = cmbConnection.SelectedItem.ToString();
        if (connectionFilter != "Все")
        {
            filteredMachines = filteredMachines.FindAll(m => m.Connection == connectionFilter);
        }

        // Фильтр по дополнительному статусу
        string extraFilter = cmbExtraStatus.SelectedItem.ToString();
        if (extraFilter != "Все")
        {
            filteredMachines = filteredMachines.FindAll(m => m.Extra == extraFilter);
        }

        UpdateTable();

        lblFilterResult.Text = filteredMachines.Count == 0 ? "⚠️ Нет автоматов, соответствующих фильтрам" : "";
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        cmbStatus.SelectedIndex = 0;
        cmbConnection.SelectedIndex = 0;
        cmbExtraStatus.SelectedIndex = 0;
        filteredMachines = new List<MachineMonitor>(allMachines);
        UpdateTable();
        lblFilterResult.Text = "";
    }

    private void UpdateTable()
    {
        // Обновляем таблицу
        dgvMachines.DataSource = null;
        dgvMachines.DataSource = filteredMachines;

        // Настройка заголовков
        if (dgvMachines.Columns.Contains("HardwareStatus"))
            dgvMachines.Columns["HardwareStatus"].HeaderText = "ТП";
        if (dgvMachines.Columns.Contains("Number"))
            dgvMachines.Columns["Number"].HeaderText = "№";
        if (dgvMachines.Columns.Contains("Name"))
            dgvMachines.Columns["Name"].HeaderText = "ТА";
        if (dgvMachines.Columns.Contains("Connection"))
            dgvMachines.Columns["Connection"].HeaderText = "Связь";
        if (dgvMachines.Columns.Contains("LoadPercent"))
            dgvMachines.Columns["LoadPercent"].HeaderText = "Загрузка, %";
        if (dgvMachines.Columns.Contains("CashAmount"))
            dgvMachines.Columns["CashAmount"].HeaderText = "Денежные средства, руб";
        if (dgvMachines.Columns.Contains("Events"))
            dgvMachines.Columns["Events"].HeaderText = "События";
        if (dgvMachines.Columns.Contains("Equipment"))
            dgvMachines.Columns["Equipment"].HeaderText = "Оборудование";
        if (dgvMachines.Columns.Contains("Info"))
            dgvMachines.Columns["Info"].HeaderText = "Информация";
        if (dgvMachines.Columns.Contains("Extra"))
            dgvMachines.Columns["Extra"].HeaderText = "Доп.";

        // Обновляем итоговую информацию
        lblTotalCount.Text = $"Итого автоматов: {filteredMachines.Count} из {allMachines.Count}";

        decimal totalMoney = 0;
        foreach (var m in filteredMachines)
        {
            totalMoney += m.CashAmount;
        }
        lblTotalMoney.Text = $"Денег в автоматах: {totalMoney:N0} руб.";

        // Если нет результатов, показываем сообщение
        if (filteredMachines.Count == 0)
        {
            dgvMachines.DataSource = null;
            var emptyTable = new DataTable();
            dgvMachines.DataSource = emptyTable;
        }
    }

    // Класс данных автомата для монитора
    public class MachineMonitor
    {
        public string HardwareStatus { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Connection { get; set; }
        public int LoadPercent { get; set; }
        public decimal CashAmount { get; set; }
        public int Events { get; set; }
        public string Equipment { get; set; }
        public string Info { get; set; }
        public string Extra { get; set; }
        public string Status { get; set; }
    }
}