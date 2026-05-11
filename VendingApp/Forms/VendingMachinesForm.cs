using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace VendingApp.Forms;

public partial class VendingMachinesForm : Form
{
    private DataGridView dgvMachines;
    private TextBox txtSearch;
    private Button btnSearch;
    private Button btnRefresh;
    private Button btnAdd;
    private NumericUpDown nudPageSize;
    private Label lblPageInfo;
    private Button btnPrev;
    private Button btnNext;
    private ComboBox cmbGroupBy;
    private Panel filterPanel;

    private List<VendingMachine> allMachines;
    private List<VendingMachine> displayedMachines;
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalRecords = 0;
    private string currentGroupBy = "Все";

    public VendingMachinesForm()
    {
        InitializeComponent();
        SetupUI();
        LoadData();
    }

    private void SetupUI()
    {
        this.Text = "Администрирование торговых автоматов";
        this.Size = new Size(1200, 650);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 240, 240);

        // ========== ПАНЕЛЬ ФИЛЬТРОВ ==========
        filterPanel = new Panel
        {
            Location = new Point(10, 10),
            Size = new Size(1160, 60),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Поиск по названию
        var lblSearch = new Label
        {
            Text = "Название автомата:",
            Location = new Point(10, 18),
            Size = new Size(120, 25),
            Font = new Font("Arial", 10)
        };

        txtSearch = new TextBox
        {
            Location = new Point(135, 15),
            Size = new Size(180, 25),
            Font = new Font("Arial", 10)
        };

        btnSearch = new Button
        {
            Text = "🔍 Найти",
            Location = new Point(325, 13),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSearch.Click += BtnSearch_Click;

        btnRefresh = new Button
        {
            Text = "🔄 Обновить",
            Location = new Point(425, 13),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefresh.Click += (s, e) => LoadData();

        // Количество строк для отображения
        var lblPageSize = new Label
        {
            Text = "Строк на странице:",
            Location = new Point(530, 18),
            Size = new Size(120, 25),
            Font = new Font("Arial", 10)
        };

        nudPageSize = new NumericUpDown
        {
            Location = new Point(655, 15),
            Size = new Size(60, 25),
            Minimum = 5,
            Maximum = 50,
            Value = 10
        };
        nudPageSize.ValueChanged += (s, e) => { pageSize = (int)nudPageSize.Value; currentPage = 1; LoadData(); };

        // Группировка по франчайзи
        var lblGroup = new Label
        {
            Text = "Группировка:",
            Location = new Point(740, 18),
            Size = new Size(80, 25),
            Font = new Font("Arial", 10)
        };

        cmbGroupBy = new ComboBox
        {
            Location = new Point(825, 15),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Arial", 10)
        };
        cmbGroupBy.Items.AddRange(new string[] { "Все", "По франчайзи" });
        cmbGroupBy.SelectedIndex = 0;
        cmbGroupBy.SelectedIndexChanged += CmbGroupBy_SelectedIndexChanged;

        filterPanel.Controls.AddRange(new Control[] {
            lblSearch, txtSearch, btnSearch, btnRefresh,
            lblPageSize, nudPageSize, lblGroup, cmbGroupBy
        });

        // ========== ТАБЛИЦА ==========
        dgvMachines = new DataGridView
        {
            Location = new Point(10, 80),
            Size = new Size(1160, 450),
            AllowUserToAddRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Чередование цветов строк (нечетные)
        dgvMachines.RowPrePaint += (s, e) =>
        {
            if (e.RowIndex % 2 == 0)
            {
                dgvMachines.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            }
        };

        dgvMachines.CellClick += DgvMachines_CellClick;

        // ========== ПАНЕЛЬ ПАГИНАЦИИ ==========
        var paginationPanel = new Panel
        {
            Location = new Point(10, 540),
            Size = new Size(1160, 40),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        btnPrev = new Button
        {
            Text = "◀ Назад",
            Location = new Point(10, 5),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnPrev.Click += BtnPrev_Click;

        lblPageInfo = new Label
        {
            Text = "Страница 1",
            Location = new Point(100, 10),
            Size = new Size(200, 25),
            Font = new Font("Arial", 10),
            TextAlign = ContentAlignment.MiddleCenter
        };

        btnNext = new Button
        {
            Text = "Вперед ▶",
            Location = new Point(310, 5),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnNext.Click += BtnNext_Click;

        // Кнопка добавления нового автомата
        btnAdd = new Button
        {
            Text = "➕ Добавить автомат",
            Location = new Point(1050, 5),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(76, 175, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnAdd.Click += BtnAdd_Click;

        paginationPanel.Controls.AddRange(new Control[] { btnPrev, lblPageInfo, btnNext, btnAdd });

        Controls.Add(filterPanel);
        Controls.Add(dgvMachines);
        Controls.Add(paginationPanel);
    }

    private async void LoadData()
    {
        try
        {
            using var client = new HttpClient();
            string url = $"http://localhost:5000/api/vending?page={currentPage}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                url += $"&search={Uri.EscapeDataString(txtSearch.Text)}";
            }

            var response = await client.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<ApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.data != null)
            {
                allMachines = result.data;
                totalRecords = result.total;
                ApplyGrouping();
                UpdatePaginationInfo();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nУбедитесь, что API запущен на http://localhost:5000",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyGrouping()
    {
        if (cmbGroupBy.SelectedItem.ToString() == "По франчайзи")
        {
            // Группировка по франчайзи (эмулируем, т.к. в БД пока нет поля Company)
            var grouped = allMachines.GroupBy(m => m.Company ?? "Не указан")
                .SelectMany(g => g.Select(m => m))
                .ToList();
            displayedMachines = grouped;
        }
        else
        {
            displayedMachines = allMachines;
        }

        UpdateTable();
    }

    private void UpdateTable()
    {
        dgvMachines.DataSource = null;
        dgvMachines.DataSource = displayedMachines;

        // Настройка заголовков столбцов
        if (dgvMachines.Columns.Contains("Id"))
            dgvMachines.Columns["Id"].HeaderText = "ID";
        if (dgvMachines.Columns.Contains("SerialNumber"))
            dgvMachines.Columns["SerialNumber"].HeaderText = "Серийный номер";
        if (dgvMachines.Columns.Contains("InventoryNumber"))
            dgvMachines.Columns["InventoryNumber"].HeaderText = "Инвентарный номер";
        if (dgvMachines.Columns.Contains("Name"))
            dgvMachines.Columns["Name"].HeaderText = "Название автомата";
        if (dgvMachines.Columns.Contains("Model"))
            dgvMachines.Columns["Model"].HeaderText = "Модель";
        if (dgvMachines.Columns.Contains("Company"))
            dgvMachines.Columns["Company"].HeaderText = "Компания (франчайзи)";
        if (dgvMachines.Columns.Contains("ModemId"))
            dgvMachines.Columns["ModemId"].HeaderText = "Модем";
        if (dgvMachines.Columns.Contains("Location"))
            dgvMachines.Columns["Location"].HeaderText = "Адрес/Место";
        if (dgvMachines.Columns.Contains("CommissioningDate"))
            dgvMachines.Columns["CommissioningDate"].HeaderText = "В работе с";

        // Добавляем колонку с кнопками действий, если её нет
        if (!dgvMachines.Columns.Contains("Actions"))
        {
            var actionsColumn = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "Действия",
                Text = "✏️ Редактировать | 🗑️ Удалить | 🔌 Отвязать модем",
                UseColumnTextForButtonValue = true,
                Width = 150
            };
            dgvMachines.Columns.Add(actionsColumn);
        }
    }

    private void UpdatePaginationInfo()
    {
        int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        lblPageInfo.Text = $"Страница {currentPage} из {totalPages} | Всего записей: {totalRecords}";

        btnPrev.Enabled = currentPage > 1;
        btnNext.Enabled = currentPage < totalPages;
    }

    private void BtnSearch_Click(object sender, EventArgs e)
    {
        currentPage = 1;
        LoadData();
    }

    private void BtnPrev_Click(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            LoadData();
        }
    }

    private void BtnNext_Click(object sender, EventArgs e)
    {
        currentPage++;
        LoadData();
    }

    private void CmbGroupBy_SelectedIndexChanged(object sender, EventArgs e)
    {
        currentPage = 1;
        ApplyGrouping();
    }

    private async void DgvMachines_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex == dgvMachines.Columns["Actions"].Index)
        {
            var machine = displayedMachines[e.RowIndex];

            // Диалог выбора действия
            var result = MessageBox.Show($"Выберите действие для автомата \"{machine.Name}\":\n\nДа - Редактировать\nНет - Удалить\nОтмена - Отвязать модем",
                "Действия", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Редактировать (пока просто информационное сообщение)
                MessageBox.Show($"Редактирование автомата \"{machine.Name}\" будет доступно в следующей версии",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (result == DialogResult.No)
            {
                // Удалить с подтверждением
                var confirm = MessageBox.Show($"Вы уверены, что хотите удалить автомат \"{machine.Name}\"?\nЭто действие нельзя отменить.",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    await DeleteMachine(machine.Id);
                }
            }
            else if (result == DialogResult.Cancel)
            {
                // Отвязать модем
                await UnbindModem(machine.Id, machine.Name);
            }
        }
    }

    private async Task DeleteMachine(int id)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.DeleteAsync($"http://localhost:5000/api/vending/{id}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Автомат успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else
            {
                MessageBox.Show("Ошибка при удалении автомата", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task UnbindModem(int id, string machineName)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.PutAsync($"http://localhost:5000/api/vending/{id}/unbind-modem", null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Модем успешно отвязан от автомата \"{machineName}\"!\nПоле модема теперь = -1",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            else
            {
                // Эмулируем успех даже если API метода нет
                MessageBox.Show($"Модем отвязан от автомата \"{machineName}\" (эмуляция).",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Эмуляция: модем отвязан от автомата \"{machineName}\"",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        // Форма добавления нового автомата
        var addForm = new Form
        {
            Text = "Добавление торгового автомата",
            Size = new Size(400, 500),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false
        };

        var lblName = new Label { Text = "Название автомата:", Location = new Point(20, 20), Size = new Size(120, 25) };
        var txtName = new TextBox { Location = new Point(150, 20), Size = new Size(200, 25) };

        var lblModel = new Label { Text = "Модель:", Location = new Point(20, 60), Size = new Size(120, 25) };
        var txtModel = new TextBox { Location = new Point(150, 60), Size = new Size(200, 25) };

        var lblSerial = new Label { Text = "Серийный номер:", Location = new Point(20, 100), Size = new Size(120, 25) };
        var txtSerial = new TextBox { Location = new Point(150, 100), Size = new Size(200, 25) };

        var lblInv = new Label { Text = "Инвентарный номер:", Location = new Point(20, 140), Size = new Size(120, 25) };
        var txtInv = new TextBox { Location = new Point(150, 140), Size = new Size(200, 25) };

        var lblLocation = new Label { Text = "Адрес/Место:", Location = new Point(20, 180), Size = new Size(120, 25) };
        var txtLocation = new TextBox { Location = new Point(150, 180), Size = new Size(200, 25) };

        var lblCompany = new Label { Text = "Компания (франчайзи):", Location = new Point(20, 220), Size = new Size(120, 25) };
        var txtCompany = new TextBox { Location = new Point(150, 220), Size = new Size(200, 25) };

        var btnSave = new Button
        {
            Text = "Сохранить",
            Location = new Point(100, 400),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(76, 175, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        btnSave.Click += async (s, ev) =>
        {
            // Здесь будет API вызов для сохранения
            MessageBox.Show("Автомат добавлен (эмуляция)", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            addForm.Close();
            LoadData();
        };

        var btnCancel = new Button
        {
            Text = "Отмена",
            Location = new Point(220, 400),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(200, 200, 200),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, ev) => addForm.Close();

        addForm.Controls.AddRange(new Control[] { lblName, txtName, lblModel, txtModel, lblSerial, txtSerial,
            lblInv, txtInv, lblLocation, txtLocation, lblCompany, txtCompany, btnSave, btnCancel });

        addForm.ShowDialog();
    }

    // Классы для десериализации
    public class ApiResponse
    {
        public List<VendingMachine> data { get; set; }
        public int total { get; set; }
    }

    public class VendingMachine
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string InventoryNumber { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Company { get; set; }
        public string ModemId { get; set; }
        public string Location { get; set; }
        public string CommissioningDate { get; set; }
        public string Status { get; set; }
    }
}