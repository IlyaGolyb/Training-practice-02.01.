using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using МозаикаERP.Forms;
using МозаикаERP.Services;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace МозаикаERP
{
    public partial class MainForm : Form
    {
        private Dictionary<string, Panel> modulePanels = new Dictionary<string, Panel>();
        private Panel currentPanel;
        private Panel navPanel; // Сохраняем ссылку на панель навигации
       
        private DataTable partnersDataTable;
        private DataTable materialsDataTable;
        private DataTable productsDataTable;
        private DataTable employeesDataTable;
        private DataTable productionDataTable;
        private DataGridView partnersGridView;

        private Label userInfoLabel;

        public MainForm()
        {
            InitializeComponent();
            SetupMainForm();
            SetupNavigation();
            CreateModulePanels();
            SwitchToModule("Партнеры");
            UpdateUIForCurrentRole(); // Обновляем интерфейс по роли
        }

        private void SetupMainForm()
        {
            this.Text = $"АСУП Мозаика-Про - {AuthService.CurrentRole}";
            this.Size = new Size(1000, 650); // Увеличили размер для кнопок
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 550);

            StyleManager.ApplyFormStyle(this);

            // Загрузка логотипа и информации о пользователе
            LoadLogoAndUserInfo();
        }

        private void LoadLogoAndUserInfo()
        {
            var headerPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, 80),
                Location = new Point(0, 0),
                BackColor = StyleManager.SecondaryBackground
            };

            var logoLabel = new Label
            {
                Text = "МОЗАИКА ERP",
                Font = new Font("Comic Sans MS", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0x54, 0x6F, 0x94),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Информация о пользователе
            userInfoLabel = new Label
            {
                Text = $"{AuthService.CurrentUser} ({AuthService.CurrentRole})",
                Font = new Font("Comic Sans MS", 10, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor,
                Location = new Point(this.ClientSize.Width - 400, 25),
                AutoSize = true
            };

            // Кнопка выхода
            var btnLogout = new Button
            {
                Text = "🚪 Выход",
                Location = new Point(this.ClientSize.Width - 200, 20),
                Size = new Size(80, 35)
            };
            StyleManager.ApplyButtonStyle(btnLogout);
            btnLogout.Click += (s, e) =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    AuthService.Logout();
                    Application.Restart();
                }
            };

            headerPanel.Controls.Add(logoLabel);
            headerPanel.Controls.Add(userInfoLabel);
            headerPanel.Controls.Add(btnLogout);

            this.Controls.Add(headerPanel);
        }

        private void UpdateUIForCurrentRole()
        {
            // Обновляем видимость кнопок навигации по роли
            foreach (Control control in navPanel.Controls)
            {
                if (control is Button button && button.Tag != null)
                {
                    string moduleName = button.Tag.ToString();

                    // Особый случай для Модуля 4
                    if (moduleName == "Модуль 4")
                    {
                        bool hasAccess = AuthService.HasAccessToModule4();
                        button.Visible = hasAccess;
                        button.Enabled = hasAccess;
                    }
                    else
                    {
                        bool hasAccess = AuthService.HasAccessToModule(moduleName);
                        button.Visible = hasAccess;
                        button.Enabled = hasAccess;
                    }
                }
            }

            // Обновляем заголовок
            this.Text = $"АСУП Мозаика-Про - {AuthService.CurrentRole}";

            if (userInfoLabel != null)
            {
                userInfoLabel.Text = $"{AuthService.CurrentUser} ({AuthService.CurrentRole})";
            }
        }

        private void SetupNavigation()
        {
            // Панель навигации слева
            navPanel = new Panel
            {
                Size = new Size(250, this.ClientSize.Height),
                Location = new Point(0, 0),
                BackColor = StyleManager.SecondaryBackground,
                Padding = new Padding(15)
            };

            // Кнопки навигации (УБРАЛИ "🏢 Склад")
            string[] modules = {
        "📊 Партнеры",
        "📝 Заявки",
        "🏭 Продукция",
        "📦 Материалы",
        "⚙️ Производство", // Было на 5 позиции, стало на 4
        "👥 Сотрудники",   // Сдвинулись
        "📈 Отчетность",
        "🧮 Модуль 4"
    };

            int y = 110;
            foreach (var module in modules)
            {
                var button = new Button
                {
                    Text = module,
                    Location = new Point(25, y),
                    Size = new Size(200, 45),
                    Font = new Font("Comic Sans MS", 11),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag = module.Replace("📊 ", "").Replace("📝 ", "").Replace("🏭 ", "")
                               .Replace("📦 ", "").Replace("⚙️ ", "")
                               .Replace("👥 ", "").Replace("📈 ", "").Replace("🧮 ", "")
                };

                StyleManager.ApplyButtonStyle(button, module.Contains("🧮"));
                button.Click += NavButton_Click;
                navPanel.Controls.Add(button);
                y += 55;
            }

            // Кнопка теста БД
            var btnTestDB = new Button
            {
                Text = "🔧 Тест подключения к БД",
                Location = new Point(25, y + 20),
                Size = new Size(200, 40),
                Font = new Font("Comic Sans MS", 10)
            };
            StyleManager.ApplyButtonStyle(btnTestDB);
            btnTestDB.Click += (s, e) => TestDatabaseConnection();
            navPanel.Controls.Add(btnTestDB);

            this.Controls.Add(navPanel);
        }

        private void CreateModulePanels()
        {
            // Создаем панели для каждого модуля
            modulePanels.Add("Партнеры", CreatePartnersPanel());
            modulePanels.Add("Заявки", CreateOrdersPanel());
            modulePanels.Add("Продукция", CreateProductsPanel());
            modulePanels.Add("Материалы", CreateMaterialsPanel());
            modulePanels.Add("Производство", CreateProductionPanel());
            modulePanels.Add("Сотрудники", CreateEmployeesPanel());
            modulePanels.Add("Отчетность", CreateReportsPanel());

            // Размещаем панели
            int panelX = navPanel.Width + 10;
            int panelWidth = this.ClientSize.Width - panelX - 10;

            foreach (var panel in modulePanels.Values)
            {
                panel.Location = new Point(panelX, 10);
                panel.Size = new Size(panelWidth, this.ClientSize.Height - 20);
                panel.Visible = false;
                this.Controls.Add(panel);
            }
        }

        private void SwitchToModule(string moduleName)
        {

            // Скрываем текущую панель
            if (currentPanel != null)
                currentPanel.Visible = false;

            // Показываем выбранную панель
            if (modulePanels.ContainsKey(moduleName))
            {
                currentPanel = modulePanels[moduleName];
                currentPanel.Visible = true;
                this.Text = $"АСУП Мозаика-Про - {moduleName}";

                LoadModuleData(moduleName);
            }
        }

        private void LoadModuleData(string moduleName)
        {
            if (currentPanel == null) return;

            DataGridView grid = FindDataGridView(currentPanel);
            if (grid == null) return;

            try
            {
                switch (moduleName)
                {
                    case "Партнеры":
                        var partnersData = DatabaseService.GetPartners();
                        if (partnersData != null && partnersData.Rows.Count > 0)
                        {
                            partnersDataTable = partnersData;
                            partnersDataTable.PrimaryKey = new DataColumn[] { partnersDataTable.Columns["ID"] };
                            partnersGridView = grid;
                            grid.DataSource = partnersDataTable;
                            AdjustGridColumns(grid);
                        }
                        else
                        {
                            CreateTestData(grid, moduleName);
                        }
                        break;

                    case "Материалы":
                        var materialsData = DatabaseService.GetMaterials();
                        if (materialsData != null && materialsData.Rows.Count > 0)
                        {
                            materialsDataTable = materialsData;
                            materialsDataTable.PrimaryKey = new DataColumn[] { materialsDataTable.Columns["ID"] };
                            grid.DataSource = materialsDataTable;
                            AdjustGridColumns(grid);
                        }
                        else
                        {
                            CreateTestData(grid, moduleName);
                        }
                        break;

                    case "Продукция":
                        var productsData = DatabaseService.GetProducts();
                        if (productsData != null && productsData.Rows.Count > 0)
                        {
                            productsDataTable = productsData;
                            productsDataTable.PrimaryKey = new DataColumn[] { productsDataTable.Columns["ID"] };
                            grid.DataSource = productsDataTable;
                            AdjustGridColumns(grid);
                        }
                        else
                        {
                            CreateTestData(grid, moduleName);
                        }
                        break;

                    case "Сотрудники":
                        var employeesData = DatabaseService.GetEmployees();
                        if (employeesData != null && employeesData.Rows.Count > 0)
                        {
                            employeesDataTable = employeesData;
                            employeesDataTable.PrimaryKey = new DataColumn[] { employeesDataTable.Columns["ID"] };
                            grid.DataSource = employeesDataTable;
                            AdjustGridColumns(grid);
                        }
                        else
                        {
                            CreateTestData(grid, moduleName);
                        }
                        break;

                    case "Производство":
                        var productionData = DatabaseService.GetProductionTasksWithMaster();
                        if (productionData != null && productionData.Rows.Count > 0)
                        {
                            productionDataTable = productionData;
                            productionDataTable.PrimaryKey = new DataColumn[] { productionDataTable.Columns["ID задания"] };
                            grid.DataSource = productionDataTable;
                            AdjustGridColumns(grid);
                        }
                        else
                        {
                            var simpleProductionData = DatabaseService.GetProductionTasksSimple();
                            if (simpleProductionData != null && simpleProductionData.Rows.Count > 0)
                            {
                                productionDataTable = simpleProductionData;
                                productionDataTable.PrimaryKey = new DataColumn[] { productionDataTable.Columns["ID задания"] };
                                grid.DataSource = productionDataTable;
                                AdjustGridColumns(grid);
                            }
                            else
                            {
                                CreateTestData(grid, moduleName);
                            }
                        }
                        break;

                    case "Заявки":
                        LoadOrdersWithFilter("Все");
                        break;

                    case "Отчетность":
                        break;

                    default:
                        CreateTestData(grid, moduleName);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CreateTestData(grid, moduleName);
            }
        }

        private DataGridView FindDataGridView(Panel panel)
        {
            foreach (Control control in panel.Controls)
            {
                if (control is DataGridView grid)
                    return grid;

                // Рекурсивный поиск в контейнерах
                if (control.HasChildren)
                {
                    foreach (Control child in control.Controls)
                    {
                        if (child is DataGridView childGrid)
                            return childGrid;
                    }
                }
            }
            return null;
        }

        private void AdjustGridColumns(DataGridView grid)
        {
            if (grid.Columns.Count == 0) return;

            // Автоматическая настройка ширины колонок
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Дополнительная настройка
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.MinimumWidth = 80;
                column.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            }
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string moduleName = button.Tag?.ToString() ??
                              button.Text.Replace("📊 ", "").Replace("📝 ", "").Replace("🏭 ", "")
                                        .Replace("📦 ", "").Replace("🏢 ", "").Replace("⚙️ ", "")
                                        .Replace("👥 ", "").Replace("📈 ", "").Replace("🧮 ", "");

            if (moduleName == "Модуль 4")
            {
                // Проверяем доступ к Модулю 4
                if (AuthService.HasAccessToModule4())
                {
                    var suppliersForm = new SuppliersForm();
                    suppliersForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("У вас нет доступа к Модулю 4!",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // Проверяем доступ к обычным модулям
                if (!AuthService.HasAccessToModule(moduleName))
                {
                    MessageBox.Show($"У вас нет доступа к модулю '{moduleName}'!",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SwitchToModule(moduleName);
            }
        }

        /// <summary>
        /// Создание панели с кнопками управления для таблиц
        /// </summary>
        private Panel CreateManagementButtonsPanel(string moduleName, DataGridView grid)
        {
            var buttonsPanel = new Panel
            {
                Location = new Point(20, 540),
                Size = new Size(850, 60),
                BackColor = Color.Transparent
            };

            // Кнопка Обновить
            var btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(0, 0),
                Size = new Size(180, 40)
            };
            StyleManager.ApplyButtonStyle(btnRefresh, true);
            btnRefresh.Click += (s, e) => LoadModuleData(moduleName);

            // Кнопка Добавить
            var btnAdd = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(190, 0),
                Size = new Size(180, 40)
            };
            StyleManager.ApplyButtonStyle(btnAdd);
            btnAdd.Click += (s, e) => ShowAddForm(moduleName, grid);

            // Кнопка Редактировать
            var btnEdit = new Button
            {
                Text = "✏️ Редактировать",
                Location = new Point(380, 0),
                Size = new Size(180, 40)
            };
            StyleManager.ApplyButtonStyle(btnEdit);
            btnEdit.Click += (s, e) => ShowEditForm(moduleName, grid);

            // Кнопка Удалить
            var btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Location = new Point(570, 0),
                Size = new Size(180, 40)
            };
            StyleManager.ApplyButtonStyle(btnDelete);
            btnDelete.Click += (s, e) => DeleteRecord(moduleName, grid);

            buttonsPanel.Controls.Add(btnRefresh);
            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnEdit);
            buttonsPanel.Controls.Add(btnDelete);

            return buttonsPanel;
        }

        private void TestDatabaseConnection()
        {
            try
            {
                string result = DatabaseService.TestConnection();
                MessageBox.Show(result, "Тест подключения к БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка теста БД: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Создание панелей модулей (С ПРАВИЛЬНЫМИ РАЗМЕРАМИ)

        private Panel CreatePartnersPanel()
        {
            var panel = new Panel
            {
                Name = "panelPartners",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление партнерами",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridPartners",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            // Передаем только grid, DataTable будет установлен в LoadModuleData
            var buttonsPanel = CreateManagementButtonsPanel("Партнеры", grid);
            panel.Controls.Add(grid);
            panel.Controls.Add(buttonsPanel);

            return panel;
        }

        /// <summary>
        /// Показ формы добавления для разных модулей
        /// </summary>
        private void ShowAddForm(string moduleName, DataGridView grid)
        {
            // Проверяем специальные модули
            if (moduleName == "Партнеры")
            {
                ShowAddPartnerForm(grid);
                return;
            }
            else if (moduleName == "Материалы")
            {
                ShowAddMaterialForm(grid);
                return;
            }
            else if (moduleName == "Производство")
            {
                ShowAddProductionForm(grid);
                return;
            }
            var addForm = new Form
            {
                Text = $"Добавление записи в {moduleName}",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(addForm);

            var title = new Label
            {
                Text = $"Новая запись в '{moduleName}'",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            var lblName = new Label { Text = "Наименование:", Location = new Point(20, 60) };
            var txtName = new TextBox { Location = new Point(150, 57), Size = new Size(200, 25) };

            var lblDescription = new Label { Text = "Описание:", Location = new Point(20, 100) };
            var txtDescription = new TextBox { Location = new Point(150, 97), Size = new Size(200, 25) };

            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(100, 150),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnSave, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(230, 150),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Получаем DataTable для модуля
                    DataTable dataTable = GetDataTableForModule(moduleName);
                    if (dataTable == null)
                    {
                        MessageBox.Show($"Нет данных для модуля '{moduleName}'", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Генерируем новый ID
                    int newId = 1;
                    if (dataTable.Rows.Count > 0)
                    {
                        int maxId = 0;
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row[0] != DBNull.Value)
                            {
                                int currentId = Convert.ToInt32(row[0]);
                                if (currentId > maxId)
                                    maxId = currentId;
                            }
                        }
                        newId = maxId + 1;
                    }

                    // Создаем новую строку
                    DataRow newRow = dataTable.NewRow();
                    newRow[0] = newId; // ID
                    newRow[1] = txtName.Text; // Название
                    if (dataTable.Columns.Count > 2)
                        newRow[2] = txtDescription.Text; // Описание

                    dataTable.Rows.Add(newRow);

                    MessageBox.Show($"Запись '{txtName.Text}' успешно добавлена! ID: {newId}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    addForm.DialogResult = DialogResult.OK;
                    addForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                addForm.DialogResult = DialogResult.Cancel;
                addForm.Close();
            };

            addForm.Controls.Add(title);
            addForm.Controls.Add(lblName);
            addForm.Controls.Add(txtName);
            addForm.Controls.Add(lblDescription);
            addForm.Controls.Add(txtDescription);
            addForm.Controls.Add(btnSave);
            addForm.Controls.Add(btnCancel);

            foreach (Control control in addForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
            }

            addForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы редактирования
        /// </summary>
        /// <summary>
        /// Показ формы редактирования
        /// </summary>
        private void ShowEditForm(string moduleName, DataGridView grid)
        {
            if (grid == null || grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем специальные модули
            if (moduleName == "Партнеры")
            {
                ShowEditPartnerForm(grid);
                return;
            }
            else if (moduleName == "Материалы")
            {
                ShowEditMaterialForm(grid);
                return;
            }
            else if (moduleName == "Производство")
            {
                ShowEditProductionForm(grid);
                return;
            }

            // Для остальных модулей
            var selectedRow = grid.SelectedRows[0];
            int recordId = Convert.ToInt32(selectedRow.Cells[0].Value);
            string recordName = selectedRow.Cells[1].Value?.ToString() ?? "";
            string recordDescription = selectedRow.Cells.Count > 2 ?
                selectedRow.Cells[2].Value?.ToString() ?? "" : "";

            var editForm = new Form
            {
                Text = $"Редактирование: {recordName}",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(editForm);

            var title = new Label
            {
                Text = $"Редактирование записи",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            var lblId = new Label { Text = "ID:", Location = new Point(20, 60) };
            var txtId = new TextBox
            {
                Text = recordId.ToString(),
                Location = new Point(150, 57),
                Size = new Size(100, 25),
                ReadOnly = true,
                BackColor = Color.LightGray
            };

            var lblName = new Label { Text = "Наименование:", Location = new Point(20, 100) };
            var txtName = new TextBox
            {
                Text = recordName,
                Location = new Point(150, 97),
                Size = new Size(200, 25)
            };

            var lblDescription = new Label { Text = "Описание:", Location = new Point(20, 140) };
            var txtDescription = new TextBox
            {
                Text = recordDescription,
                Location = new Point(150, 137),
                Size = new Size(200, 25)
            };

            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(100, 180),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnSave, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(230, 180),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    DataTable dataTable = GetDataTableForModule(moduleName);
                    if (dataTable == null) return;

                    // Находим строку по ID
                    DataRow[] rows = dataTable.Select($"{dataTable.Columns[0].ColumnName} = {recordId}");
                    if (rows.Length > 0)
                    {
                        DataRow rowToUpdate = rows[0];
                        rowToUpdate.BeginEdit();
                        rowToUpdate[1] = txtName.Text; // Название
                        if (dataTable.Columns.Count > 2)
                            rowToUpdate[2] = txtDescription.Text; // Описание
                        rowToUpdate.EndEdit();

                        MessageBox.Show($"Запись '{recordName}' успешно обновлена!",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        editForm.DialogResult = DialogResult.OK;
                        editForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                editForm.DialogResult = DialogResult.Cancel;
                editForm.Close();
            };

            editForm.Controls.Add(title);
            editForm.Controls.Add(lblId);
            editForm.Controls.Add(txtId);
            editForm.Controls.Add(lblName);
            editForm.Controls.Add(txtName);
            editForm.Controls.Add(lblDescription);
            editForm.Controls.Add(txtDescription);
            editForm.Controls.Add(btnSave);
            editForm.Controls.Add(btnCancel);

            foreach (Control control in editForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
            }

            editForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы добавления материала
        /// </summary>
        private void ShowAddMaterialForm(DataGridView grid)
        {
            var addForm = new Form
            {
                Text = "Добавление нового материала",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(addForm);

            var title = new Label
            {
                Text = "Новый материал",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            int y = 60;

            // Наименование
            var lblName = new Label { Text = "Наименование:", Location = new Point(20, y) };
            var txtName = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25) };
            y += 40;

            // Тип
            var lblType = new Label { Text = "Тип:", Location = new Point(20, y) };
            var cmbType = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new object[] { "Глина", "Глазурь", "Пигмент", "Упаковка", "Другое" });
            cmbType.SelectedIndex = 0;
            y += 40;

            // Поставщик (было "Тестовид")
            var lblSupplier = new Label { Text = "Поставщик:", Location = new Point(20, y) };
            var txtSupplier = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25) };
            y += 40;

            // В упаковке
            var lblPackage = new Label { Text = "В упаковке:", Location = new Point(20, y) };
            var numPackage = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = 1.00m
            };
            y += 40;

            // Единица измерения
            var lblUnit = new Label { Text = "Единица:", Location = new Point(20, y) };
            var cmbUnit = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUnit.Items.AddRange(new object[] { "кг", "шт", "л", "м" });
            cmbUnit.SelectedIndex = 0;
            y += 40;

            // Цена
            var lblPrice = new Label { Text = "Цена:", Location = new Point(20, y) };
            var numPrice = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Value = 100.00m
            };
            y += 50;

            // Кнопки
            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnSave, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование материала!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Генерируем новый ID
                    int newId = 1;
                    if (materialsDataTable != null && materialsDataTable.Rows.Count > 0)
                    {
                        int maxId = 0;
                        foreach (DataRow row in materialsDataTable.Rows)
                        {
                            if (row["ID"] != DBNull.Value)
                            {
                                int currentId = Convert.ToInt32(row["ID"]);
                                if (currentId > maxId)
                                    maxId = currentId;
                            }
                        }
                        newId = maxId + 1;
                    }

                    // Создаем новую строку
                    DataRow newRow = materialsDataTable.NewRow();
                    newRow["ID"] = newId;
                    newRow["Наименование"] = txtName.Text;
                    newRow["Тип"] = cmbType.Text;
                    newRow["Поставщик"] = txtSupplier.Text; // Исправлено на "Поставщик"
                    newRow["В упаковке"] = numPackage.Value;
                    newRow["Единица"] = cmbUnit.Text;
                    newRow["Цена"] = numPrice.Value;

                    materialsDataTable.Rows.Add(newRow);

                    MessageBox.Show($"Материал '{txtName.Text}' успешно добавлен! ID: {newId}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    addForm.DialogResult = DialogResult.OK;
                    addForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                addForm.DialogResult = DialogResult.Cancel;
                addForm.Close();
            };

            addForm.Controls.Add(title);
            addForm.Controls.AddRange(new Control[] { lblName, txtName, lblType, cmbType,
                        lblSupplier, txtSupplier, lblPackage, numPackage,
                        lblUnit, cmbUnit, lblPrice, numPrice, btnSave, btnCancel });

            foreach (Control control in addForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            addForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы редактирования материала
        /// </summary>
        private void ShowEditMaterialForm(DataGridView grid)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите материал для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = grid.SelectedRows[0];
            int materialId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
            string name = selectedRow.Cells["Наименование"].Value?.ToString() ?? "";
            string type = selectedRow.Cells["Тип"].Value?.ToString() ?? "";
            string supplier = selectedRow.Cells["Поставщик"].Value?.ToString() ?? ""; // Исправлено на "Поставщик"
            decimal package = 1.00m;
            if (selectedRow.Cells["В упаковке"].Value != DBNull.Value && selectedRow.Cells["В упаковке"].Value != null)
                package = Convert.ToDecimal(selectedRow.Cells["В упаковке"].Value);

            string unit = selectedRow.Cells["Единица"].Value?.ToString() ?? "кг";
            decimal price = 100.00m;
            if (selectedRow.Cells["Цена"].Value != DBNull.Value && selectedRow.Cells["Цена"].Value != null)
                price = Convert.ToDecimal(selectedRow.Cells["Цена"].Value);

            var editForm = new Form
            {
                Text = $"Редактирование: {name}",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(editForm);

            var title = new Label
            {
                Text = $"Редактирование материала",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            int y = 60;

            // ID (только для просмотра)
            var lblId = new Label { Text = "ID:", Location = new Point(20, y) };
            var txtId = new TextBox
            {
                Text = materialId.ToString(),
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            y += 40;

            // Наименование
            var lblName = new Label { Text = "Наименование:", Location = new Point(20, y) };
            var txtName = new TextBox
            {
                Text = name,
                Location = new Point(180, y - 3),
                Size = new Size(250, 25)
            };
            y += 40;

            // Тип
            var lblType = new Label { Text = "Тип:", Location = new Point(20, y) };
            var cmbType = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new object[] { "Глина", "Глазурь", "Пигмент", "Упаковка", "Другое" });
            if (!string.IsNullOrEmpty(type))
            {
                cmbType.SelectedItem = type;
            }
            if (cmbType.SelectedIndex == -1) cmbType.SelectedIndex = 0;
            y += 40;

            // Поставщик (было "Тестовид")
            var lblSupplier = new Label { Text = "Поставщик:", Location = new Point(20, y) };
            var txtSupplier = new TextBox
            {
                Text = supplier,
                Location = new Point(180, y - 3),
                Size = new Size(250, 25)
            };
            y += 40;

            // В упаковке
            var lblPackage = new Label { Text = "В упаковке:", Location = new Point(20, y) };
            var numPackage = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = package
            };
            y += 40;

            // Единица измерения
            var lblUnit = new Label { Text = "Единица:", Location = new Point(20, y) };
            var cmbUnit = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUnit.Items.AddRange(new object[] { "кг", "шт", "л", "м" });
            if (!string.IsNullOrEmpty(unit))
            {
                cmbUnit.SelectedItem = unit;
            }
            if (cmbUnit.SelectedIndex == -1) cmbUnit.SelectedIndex = 0;
            y += 40;

            // Цена
            var lblPrice = new Label { Text = "Цена:", Location = new Point(20, y) };
            var numPrice = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Value = price
            };
            y += 50;

            // Кнопки
            var btnUpdate = new Button
            {
                Text = "💾 Обновить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnUpdate, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnUpdate.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование материала!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Находим строку по ID
                    if (materialsDataTable != null)
                    {
                        DataRow[] rows = materialsDataTable.Select($"ID = {materialId}");
                        if (rows.Length > 0)
                        {
                            DataRow rowToUpdate = rows[0];
                            rowToUpdate.BeginEdit();
                            rowToUpdate["Наименование"] = txtName.Text;
                            rowToUpdate["Тип"] = cmbType.Text;
                            rowToUpdate["Поставщик"] = txtSupplier.Text; // Исправлено на "Поставщик"
                            rowToUpdate["В упаковке"] = numPackage.Value;
                            rowToUpdate["Единица"] = cmbUnit.Text;
                            rowToUpdate["Цена"] = numPrice.Value;
                            rowToUpdate.EndEdit();

                            MessageBox.Show($"Данные материала '{name}' успешно обновлены!",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            editForm.DialogResult = DialogResult.OK;
                            editForm.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                editForm.DialogResult = DialogResult.Cancel;
                editForm.Close();
            };

            editForm.Controls.Add(title);
            editForm.Controls.AddRange(new Control[] { lblId, txtId, lblName, txtName, lblType, cmbType,
                          lblSupplier, txtSupplier, lblPackage, numPackage,
                          lblUnit, cmbUnit, lblPrice, numPrice, btnUpdate, btnCancel });

            foreach (Control control in editForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            editForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы добавления задания производства
        /// </summary>
        private void ShowAddProductionForm(DataGridView grid)
        {
            var addForm = new Form
            {
                Text = "Добавление нового задания производства",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(addForm);

            var title = new Label
            {
                Text = "Новое задание производства",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            int y = 60;

            // ID заявки
            var lblOrderId = new Label { Text = "ID заявки:", Location = new Point(20, y) };
            var numOrderId = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10000,
                Value = 100
            };
            y += 40;

            // Мастер
            var lblMaster = new Label { Text = "Мастер:", Location = new Point(20, y) };
            var txtMaster = new TextBox { Location = new Point(180, y - 3), Size = new Size(200, 25) };
            y += 40;

            // Дата создания
            var lblCreateDate = new Label { Text = "Дата создания:", Location = new Point(20, y) };
            var dtpCreateDate = new DateTimePicker
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };
            y += 40;

            // Срок выполнения
            var lblDueDate = new Label { Text = "Срок выполнения:", Location = new Point(20, y) };
            var dtpDueDate = new DateTimePicker
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                Value = DateTime.Now.AddDays(7),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy"
            };
            y += 40;

            // Статус
            var lblStatus = new Label { Text = "Статус:", Location = new Point(20, y) };
            var cmbStatus = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Назначено", "В работе", "Завершено", "Отменено" });
            cmbStatus.SelectedIndex = 0;
            y += 40;

            // Приоритет
            var lblPriority = new Label { Text = "Приоритет:", Location = new Point(20, y) };
            var numPriority = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 5,
                Value = 1
            };
            y += 50;

            // Кнопки
            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnSave, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtMaster.Text))
                {
                    MessageBox.Show("Введите ФИО мастера!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Генерируем новый ID задания
                    int newId = 1;
                    if (productionDataTable != null && productionDataTable.Rows.Count > 0)
                    {
                        int maxId = 0;
                        foreach (DataRow row in productionDataTable.Rows)
                        {
                            if (row["ID задания"] != DBNull.Value)
                            {
                                int currentId = Convert.ToInt32(row["ID задания"]);
                                if (currentId > maxId)
                                    maxId = currentId;
                            }
                        }
                        newId = maxId + 1;
                    }

                    // Создаем новую строку
                    DataRow newRow = productionDataTable.NewRow();
                    newRow["ID задания"] = newId;
                    newRow["ID заявки"] = numOrderId.Value;
                    newRow["Мастер"] = txtMaster.Text;
                    newRow["Дата создания"] = dtpCreateDate.Value.ToString("dd.MM.yyyy HH:mm");
                    newRow["Срок выполнения"] = dtpDueDate.Value.ToString("dd.MM.yyyy");
                    newRow["Статус"] = cmbStatus.Text;
                    newRow["Приоритет"] = numPriority.Value;

                    productionDataTable.Rows.Add(newRow);

                    MessageBox.Show($"Задание производства успешно добавлено! ID задания: {newId}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    addForm.DialogResult = DialogResult.OK;
                    addForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                addForm.DialogResult = DialogResult.Cancel;
                addForm.Close();
            };

            addForm.Controls.Add(title);
            addForm.Controls.AddRange(new Control[] { lblOrderId, numOrderId, lblMaster, txtMaster,
                        lblCreateDate, dtpCreateDate, lblDueDate, dtpDueDate,
                        lblStatus, cmbStatus, lblPriority, numPriority,
                        btnSave, btnCancel });

            foreach (Control control in addForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is DateTimePicker dtp)
                {
                    dtp.Font = new Font("Comic Sans MS", 10);
                    dtp.BackColor = Color.White;
                    dtp.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                }
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            addForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы редактирования задания производства
        /// </summary>
        private void ShowEditProductionForm(DataGridView grid)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите задание для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = grid.SelectedRows[0];
            int taskId = Convert.ToInt32(selectedRow.Cells["ID задания"].Value);
            int orderId = Convert.ToInt32(selectedRow.Cells["ID заявки"].Value);
            string master = selectedRow.Cells["Мастер"].Value?.ToString() ?? "";
            string createDateStr = selectedRow.Cells["Дата создания"].Value?.ToString() ?? "";
            string dueDateStr = selectedRow.Cells["Срок выполнения"].Value?.ToString() ?? "";
            string status = selectedRow.Cells["Статус"].Value?.ToString() ?? "";
            int priority = 1;
            if (selectedRow.Cells["Приоритет"].Value != DBNull.Value && selectedRow.Cells["Приоритет"].Value != null)
                priority = Convert.ToInt32(selectedRow.Cells["Приоритет"].Value);

            DateTime createDate;
            if (!DateTime.TryParse(createDateStr, out createDate))
                createDate = DateTime.Now;

            DateTime dueDate;
            if (!DateTime.TryParse(dueDateStr, out dueDate))
                dueDate = DateTime.Now.AddDays(7);

            var editForm = new Form
            {
                Text = $"Редактирование задания: {taskId}",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            StyleManager.ApplyFormStyle(editForm);

            var title = new Label
            {
                Text = $"Редактирование задания производства",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            int y = 60;

            // ID задания (только для просмотра)
            var lblTaskId = new Label { Text = "ID задания:", Location = new Point(20, y) };
            var txtTaskId = new TextBox
            {
                Text = taskId.ToString(),
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            y += 40;

            // ID заявки
            var lblOrderId = new Label { Text = "ID заявки:", Location = new Point(20, y) };
            var numOrderId = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10000,
                Value = orderId
            };
            y += 40;

            // Мастер
            var lblMaster = new Label { Text = "Мастер:", Location = new Point(20, y) };
            var txtMaster = new TextBox
            {
                Text = master,
                Location = new Point(180, y - 3),
                Size = new Size(200, 25)
            };
            y += 40;

            // Дата создания
            var lblCreateDate = new Label { Text = "Дата создания:", Location = new Point(20, y) };
            var dtpCreateDate = new DateTimePicker
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                Value = createDate,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };
            y += 40;

            // Срок выполнения
            var lblDueDate = new Label { Text = "Срок выполнения:", Location = new Point(20, y) };
            var dtpDueDate = new DateTimePicker
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                Value = dueDate,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy"
            };
            y += 40;

            // Статус
            var lblStatus = new Label { Text = "Статус:", Location = new Point(20, y) };
            var cmbStatus = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Назначено", "В работе", "Завершено", "Отменено" });
            if (!string.IsNullOrEmpty(status))
            {
                cmbStatus.SelectedItem = status;
            }
            if (cmbStatus.SelectedIndex == -1) cmbStatus.SelectedIndex = 0;
            y += 40;

            // Приоритет
            var lblPriority = new Label { Text = "Приоритет:", Location = new Point(20, y) };
            var numPriority = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 5,
                Value = priority
            };
            y += 50;

            // Кнопки
            var btnUpdate = new Button
            {
                Text = "💾 Обновить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnUpdate, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            btnUpdate.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtMaster.Text))
                {
                    MessageBox.Show("Введите ФИО мастера!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Находим строку по ID задания
                    if (productionDataTable != null)
                    {
                        DataRow[] rows = productionDataTable.Select($"[ID задания] = {taskId}");
                        if (rows.Length > 0)
                        {
                            DataRow rowToUpdate = rows[0];
                            rowToUpdate.BeginEdit();
                            rowToUpdate["ID заявки"] = numOrderId.Value;
                            rowToUpdate["Мастер"] = txtMaster.Text;
                            rowToUpdate["Дата создания"] = dtpCreateDate.Value.ToString("dd.MM.yyyy HH:mm");
                            rowToUpdate["Срок выполнения"] = dtpDueDate.Value.ToString("dd.MM.yyyy");
                            rowToUpdate["Статус"] = cmbStatus.Text;
                            rowToUpdate["Приоритет"] = numPriority.Value;
                            rowToUpdate.EndEdit();

                            MessageBox.Show($"Данные задания {taskId} успешно обновлены!",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            editForm.DialogResult = DialogResult.OK;
                            editForm.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                editForm.DialogResult = DialogResult.Cancel;
                editForm.Close();
            };

            editForm.Controls.Add(title);
            editForm.Controls.AddRange(new Control[] { lblTaskId, txtTaskId, lblOrderId, numOrderId, lblMaster, txtMaster,
                          lblCreateDate, dtpCreateDate, lblDueDate, dtpDueDate,
                          lblStatus, cmbStatus, lblPriority, numPriority,
                          btnUpdate, btnCancel });

            foreach (Control control in editForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is DateTimePicker dtp)
                {
                    dtp.Font = new Font("Comic Sans MS", 10);
                    dtp.BackColor = Color.White;
                    dtp.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                }
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            editForm.ShowDialog();
        }

        /// <summary>
        /// Удаление записи
        /// </summary>
        private void DeleteRecord(string moduleName, DataGridView grid)
        {
            if (grid == null || grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем, доступно ли удаление для этого модуля
            if (moduleName == "Продукция" || moduleName == "Сотрудники")
            {
                MessageBox.Show($"Удаление записей в модуле '{moduleName}' доступно только через базу данных!",
                    "Удаление запрещено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = grid.SelectedRows[0];
            string recordName = selectedRow.Cells[1]?.Value?.ToString() ?? "выбранную запись";
            int recordId = Convert.ToInt32(selectedRow.Cells[0].Value);

            var result = MessageBox.Show($"Вы уверены, что хотите удалить запись:\n" +
                                       $"'{recordName}' (ID: {recordId}) из модуля '{moduleName}'?",
                                       "Подтверждение удаления",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DataTable dataTable = GetDataTableForModule(moduleName);
                    if (dataTable != null)
                    {
                        // Удаляем из DataTable
                        DataRow[] rows = dataTable.Select($"{dataTable.Columns[0].ColumnName} = {recordId}");
                        foreach (DataRow row in rows)
                        {
                            dataTable.Rows.Remove(row);
                        }
                    }

                    MessageBox.Show($"Запись '{recordName}' удалена из модуля '{moduleName}'",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Получение DataTable для модуля
        /// </summary>
        private DataTable GetDataTableForModule(string moduleName)
        {
            switch (moduleName)
            {
                case "Партнеры": return partnersDataTable;
                case "Материалы": return materialsDataTable;
                case "Продукция": return productsDataTable;
                case "Сотрудники": return employeesDataTable;
                case "Производство": return productionDataTable;
                default: return null;
            }
        }

        /// <summary>
        /// Показ формы добавления партнера (отдельный метод для партнеров)
        /// </summary>
        private void ShowAddPartnerForm(DataGridView grid)
        {
            var addForm = new Form
            {
                Text = "Добавление нового партнера",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            StyleManager.ApplyFormStyle(addForm);
            addForm.BackColor = StyleManager.SecondaryBackground;

            // Заголовок
            var title = new Label
            {
                Text = "Новый партнер",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            // Поля для ввода
            int y = 60;

            var lblCompany = new Label { Text = "Название компании:", Location = new Point(20, y) };
            var txtCompany = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25) };
            y += 40;

            var lblType = new Label { Text = "Тип партнера:", Location = new Point(20, y) };
            var cmbType = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Оптовый", "Розничный", "Дистрибьютор", "Монтажник", "Другой" }
            };
            cmbType.SelectedIndex = 0;
            y += 40;

            var lblPhone = new Label { Text = "Телефон:", Location = new Point(20, y) };
            var txtPhone = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25) };
            y += 40;

            var lblEmail = new Label { Text = "Email:", Location = new Point(20, y) };
            var txtEmail = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25) };
            y += 40;

            var lblRating = new Label { Text = "Начальный рейтинг:", Location = new Point(20, y) };
            var numRating = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 5,
                DecimalPlaces = 1,
                Value = 5.0m
            };
            y += 50;

            // Кнопки
            var btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnSave, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            // Обработчик сохранения
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtCompany.Text))
                {
                    MessageBox.Show("Введите название компании!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Генерируем новый ID
                    int newId = 1;
                    if (partnersDataTable != null && partnersDataTable.Rows.Count > 0)
                    {
                        int maxId = 0;
                        foreach (DataRow row in partnersDataTable.Rows)
                        {
                            int currentId = Convert.ToInt32(row["ID"]);
                            if (currentId > maxId)
                                maxId = currentId;
                        }
                        newId = maxId + 1;
                    }

                    // Создаем новую строку
                    DataRow newRow = partnersDataTable.NewRow();
                    newRow["ID"] = newId;
                    newRow["Компания"] = txtCompany.Text;
                    newRow["Тип"] = cmbType.Text;
                    newRow["Телефон"] = txtPhone.Text;
                    newRow["Email"] = txtEmail.Text;
                    newRow["Рейтинг"] = numRating.Value;
                    newRow["Дата регистрации"] = DateTime.Now.ToString("dd.MM.yyyy");

                    partnersDataTable.Rows.Add(newRow);

                    MessageBox.Show($"Партнер '{txtCompany.Text}' успешно добавлен! ID: {newId}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    addForm.DialogResult = DialogResult.OK;
                    addForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                addForm.DialogResult = DialogResult.Cancel;
                addForm.Close();
            };

            // Добавляем элементы
            addForm.Controls.Add(title);
            addForm.Controls.AddRange(new Control[] { lblCompany, txtCompany, lblType, cmbType,
                            lblPhone, txtPhone, lblEmail, txtEmail,
                            lblRating, numRating, btnSave, btnCancel });

            // Применяем стили
            foreach (Control control in addForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            addForm.ShowDialog();
        }

        /// <summary>
        /// Показ формы редактирования партнера (отдельный метод для партнеров)
        /// </summary>
        private void ShowEditPartnerForm(DataGridView grid)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите партнера для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем данные выбранного партнера
            DataGridViewRow selectedRow = grid.SelectedRows[0];
            int partnerId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
            string companyName = selectedRow.Cells["Компания"].Value?.ToString() ?? "";
            string partnerType = selectedRow.Cells["Тип"].Value?.ToString() ?? "";
            decimal rating = Convert.ToDecimal(selectedRow.Cells["Рейтинг"].Value ?? 5.0m);
            string phone = selectedRow.Cells["Телефон"].Value?.ToString() ?? "";
            string email = selectedRow.Cells["Email"].Value?.ToString() ?? "";

            // Создаем форму редактирования
            var editForm = new Form
            {
                Text = $"Редактирование: {companyName}",
                Size = new Size(500, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };
            StyleManager.ApplyFormStyle(editForm);

            // Заголовок
            var title = new Label
            {
                Text = $"Редактирование партнера",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            // Поля формы
            int y = 60;

            // ID (только для просмотра)
            var lblId = new Label { Text = "ID:", Location = new Point(20, y) };
            var txtId = new TextBox
            {
                Text = partnerId.ToString(),
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            y += 40;

            // Название компании
            var lblCompany = new Label { Text = "Название компании:", Location = new Point(20, y) };
            var txtCompany = new TextBox
            {
                Text = companyName,
                Location = new Point(180, y - 3),
                Size = new Size(250, 25)
            };
            y += 40;

            // Тип партнера
            var lblType = new Label { Text = "Тип партнера:", Location = new Point(20, y) };
            var cmbType = new ComboBox
            {
                Location = new Point(180, y - 3),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Оптовый", "Розничный", "Дистрибьютор", "Монтажник", "Другой" }
            };
            cmbType.SelectedItem = partnerType;
            if (cmbType.SelectedIndex == -1) cmbType.SelectedIndex = 0;
            y += 40;

            // Рейтинг
            var lblRating = new Label { Text = "Рейтинг:", Location = new Point(20, y) };
            var numRating = new NumericUpDown
            {
                Location = new Point(180, y - 3),
                Size = new Size(100, 25),
                Minimum = 0,
                Maximum = 5,
                DecimalPlaces = 1,
                Value = rating
            };
            y += 40;

            // Телефон
            var lblPhone = new Label { Text = "Телефон:", Location = new Point(20, y) };
            var txtPhone = new TextBox
            {
                Text = phone,
                Location = new Point(180, y - 3),
                Size = new Size(250, 25)
            };
            y += 40;

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, y) };
            var txtEmail = new TextBox
            {
                Text = email,
                Location = new Point(180, y - 3),
                Size = new Size(250, 25)
            };
            y += 50;

            // Кнопки
            var btnUpdate = new Button
            {
                Text = "💾 Обновить",
                Location = new Point(150, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnUpdate, true);

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(280, y),
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnCancel);

            // Обработчик обновления
            btnUpdate.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtCompany.Text))
                {
                    MessageBox.Show("Название компании не может быть пустым!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Находим строку по ID в DataTable
                    DataRow[] rows = partnersDataTable.Select($"ID = {partnerId}");
                    if (rows.Length > 0)
                    {
                        DataRow rowToUpdate = rows[0];
                        rowToUpdate.BeginEdit();
                        rowToUpdate["Компания"] = txtCompany.Text;
                        rowToUpdate["Тип"] = cmbType.Text;
                        rowToUpdate["Телефон"] = txtPhone.Text;
                        rowToUpdate["Email"] = txtEmail.Text;
                        rowToUpdate["Рейтинг"] = numRating.Value;
                        rowToUpdate.EndEdit();

                        MessageBox.Show($"Данные партнера '{companyName}' успешно обновлены!",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        editForm.DialogResult = DialogResult.OK;
                        editForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) =>
            {
                editForm.DialogResult = DialogResult.Cancel;
                editForm.Close();
            };

            // Добавляем элементы
            editForm.Controls.Add(title);
            editForm.Controls.AddRange(new Control[] { lblId, txtId, lblCompany, txtCompany,
                              lblType, cmbType, lblRating, numRating,
                              lblPhone, txtPhone, lblEmail, txtEmail,
                              btnUpdate, btnCancel });

            // Применяем стили
            foreach (Control control in editForm.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
                else if (control is ComboBox comboBox) StyleManager.ApplyComboBoxStyle(comboBox);
                else if (control is NumericUpDown numeric) StyleManager.ApplyNumericUpDownStyle(numeric);
            }

            editForm.ShowDialog();
        }

        private Panel CreateOrdersPanel()
        {
            var panel = new Panel
            {
                Name = "panelOrders",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление заявками",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridOrders",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            // Панель фильтров
            var filterPanel = new Panel
            {
                Location = new Point(20, 540),
                Size = new Size(600, 50),
                BackColor = Color.Transparent
            };

            // Фильтры
            var lblFilter = new Label
            {
                Text = "Фильтр по статусу:",
                Location = new Point(0, 15),
                Size = new Size(130, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            var cmbStatus = new ComboBox
            {
                Name = "cmbStatus",
                Location = new Point(140, 10),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Comic Sans MS", 10)
            };

            cmbStatus.Items.AddRange(new object[] { "Все", "НОВАЯ", "В ПРОИЗВОДСТВЕ", "ГОТОВО", "ОТГРУЖЕНО", "ОТМЕНЕНА" });
            cmbStatus.SelectedIndex = 0;

            // Обработчик изменения фильтра
            cmbStatus.SelectedIndexChanged += (s, e) =>
            {
                // Обновляем данные при изменении фильтра
                LoadOrdersWithFilter(cmbStatus.SelectedItem?.ToString() ?? "Все");
            };

            // УДАЛЕНО: Кнопка "Загрузить" 
            // Вместо нее добавим кнопку обновления
            var btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(350, 10),
                Size = new Size(150, 30)
            };
            StyleManager.ApplyButtonStyle(btnRefresh, true);
            btnRefresh.Click += (s, e) =>
            {
                LoadOrdersWithFilter(cmbStatus.SelectedItem?.ToString() ?? "Все");
            };

            filterPanel.Controls.Add(lblFilter);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(btnRefresh);

            StyleManager.ApplyLabelStyle(lblFilter);
            StyleManager.ApplyComboBoxStyle(cmbStatus);

            panel.Controls.Add(grid);
            panel.Controls.Add(filterPanel);

            return panel;
        }

        /// <summary>
        /// Загрузка заявок с фильтром по статусу
        /// </summary>
        private void LoadOrdersWithFilter(string statusFilter)
        {
            try
            {
                DataTable ordersData = null;

                // Если подключение к БД доступно, загружаем из БД
                if (DatabaseService.IsDatabaseAvailable())
                {
                    ordersData = DatabaseService.GetOrders(statusFilter);
                }

                // Если данных из БД нет или БД недоступна, создаем тестовые данные
                if (ordersData == null || ordersData.Rows.Count == 0)
                {
                    ordersData = CreateTestOrdersData(statusFilter);
                }

                // Находим таблицу заявок
                var ordersPanel = modulePanels["Заявки"];
                if (ordersPanel != null)
                {
                    DataGridView grid = FindDataGridView(ordersPanel);
                    if (grid != null)
                    {
                        grid.DataSource = ordersData;
                        AdjustGridColumns(grid);

                        // Обновляем информацию о количестве
                        string filterInfo = statusFilter == "Все" ? "" : $" (фильтр: {statusFilter})";
                        UpdateOrdersStatusLabel($"Загружено записей: {ordersData.Rows.Count}{filterInfo}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создание тестовых данных заявок с фильтром
        /// </summary>
        private DataTable CreateTestOrdersData(string statusFilter)
        {
            var dt = new DataTable();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Партнер", typeof(string));
            dt.Columns.Add("Дата создания", typeof(string));
            dt.Columns.Add("Сумма", typeof(decimal));
            dt.Columns.Add("Статус", typeof(string));
            dt.Columns.Add("Менеджер", typeof(string));

            // Все тестовые данные
            var allOrders = new List<object[]>
    {
        new object[] {1, "ООО 'СтройТорг'", "15.12.2023", 125000.50m, "НОВАЯ", "Иванов И.И."},
        new object[] {2, "АО 'Керамика Плюс'", "16.12.2023", 89000.00m, "В ПРОИЗВОДСТВЕ", "Петров П.П."},
        new object[] {3, "ИП 'Мастерская отделки'", "17.12.2023", 45600.00m, "ГОТОВО", "Сидоров С.С."},
        new object[] {4, "ООО 'ДомСтрой'", "18.12.2023", 234500.00m, "ОТГРУЖЕНО", "Иванов И.И."},
        new object[] {5, "АО 'СтройМатериалы'", "19.12.2023", 67800.00m, "ОТМЕНЕНА", "Петров П.П."},
        new object[] {6, "ИП 'Ремонт Сервис'", "20.12.2023", 12300.00m, "НОВАЯ", "Сидоров С.С."},
        new object[] {7, "ООО 'ЭлитСтрой'", "21.12.2023", 189000.00m, "В ПРОИЗВОДСТВЕ", "Иванов И.И."},
        new object[] {8, "АО 'Современный Дизайн'", "22.12.2023", 56700.00m, "ГОТОВО", "Петров П.П."}
    };

            // Фильтрация данных
            foreach (var order in allOrders)
            {
                string orderStatus = order[4].ToString();

                if (statusFilter == "Все" || orderStatus == statusFilter)
                {
                    dt.Rows.Add(order);
                }
            }

            return dt;
        }

        /// <summary>
        /// Обновление статусной надписи для заявок
        /// </summary>
        private void UpdateOrdersStatusLabel(string message)
        {
            var ordersPanel = modulePanels["Заявки"];
            if (ordersPanel == null) return;

            // Ищем или создаем лейбл для статуса
            Label statusLabel = null;
            foreach (Control control in ordersPanel.Controls)
            {
                if (control is Label label && label.Name == "lblOrdersStatus")
                {
                    statusLabel = label;
                    break;
                }
            }

            if (statusLabel == null)
            {
                statusLabel = new Label
                {
                    Name = "lblOrdersStatus",
                    Location = new Point(20, 50),
                    AutoSize = true,
                    Font = new Font("Comic Sans MS", 9),
                    ForeColor = Color.Gray
                };
                ordersPanel.Controls.Add(statusLabel);
            }

            statusLabel.Text = message;
        }

        private Panel CreateProductsPanel()
        {
            var panel = new Panel
            {
                Name = "panelProducts",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление продукцией",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            // Добавим информационное сообщение
            var infoLabel = new Label
            {
                Text = "Управление продукцией доступно только через базу данных",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 10),
                ForeColor = Color.Gray
            };
            panel.Controls.Add(infoLabel);

            var grid = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridProducts",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            // УБРАНО: Кнопки управления для Продукции (только через БД)
            panel.Controls.Add(grid);

            return panel;
        }

        private Panel CreateMaterialsPanel()
        {
            var panel = new Panel
            {
                Name = "panelMaterials",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление материалами",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridMaterials",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            // Кнопки управления остаются для Материалов
            var buttonsPanel = CreateManagementButtonsPanel("Материалы", grid);
            panel.Controls.Add(grid);
            panel.Controls.Add(buttonsPanel);

            // Кнопка для Модуля 4
            var btnSuppliers = new Button
            {
                Text = "👥 Поставщики материала (Модуль 4)",
                Location = new Point(20, 610),
                Size = new Size(350, 45),
                Font = new Font("Comic Sans MS", 11)
            };

            bool hasModule4Access = AuthService.HasAccessToModule4();
            btnSuppliers.Enabled = hasModule4Access;

            StyleManager.ApplyButtonStyle(btnSuppliers, hasModule4Access);

            btnSuppliers.Click += (s, e) =>
            {
                if (hasModule4Access)
                {
                    var suppliersForm = new SuppliersForm();
                    suppliersForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("У вас нет доступа к Модулю 4!",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            if (!hasModule4Access)
            {
                btnSuppliers.BackColor = Color.LightGray;
                btnSuppliers.ForeColor = Color.Gray;
                btnSuppliers.Cursor = Cursors.Default;
            }

            panel.Controls.Add(btnSuppliers);

            return panel;
        }

        private Panel CreateWarehousePanel()
        {
            var panel = new Panel
            {
                Name = "panelWarehouse",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Складской учет",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridWarehouse",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            panel.Controls.Add(grid);
            return panel;
        }

        private Panel CreateProductionPanel()
        {
            var panel = new Panel
            {
                Name = "panelProduction",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление производством",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridProduction",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Для множественного выбора
                MultiSelect = true, // Разрешаем множественный выбор
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            // Кнопки управления
            var buttonsPanel = CreateManagementButtonsPanel("Производство", grid);
            panel.Controls.Add(grid);
            panel.Controls.Add(buttonsPanel);

            return panel;
        }

        private Panel CreateEmployeesPanel()
        {
            var panel = new Panel
            {
                Name = "panelEmployees",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Управление персоналом",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            var grid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(panel.Width + 350, 450),
                Name = "gridEmployees",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyleManager.ApplyGridStyle(grid);

            panel.Controls.Add(grid);
            return panel;
        }

        private Panel CreateReportsPanel()
        {
            var panel = new Panel
            {
                Name = "panelReports",
                BackColor = StyleManager.PrimaryBackground,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var title = new Label
            {
                Text = "Отчетность и аналитика",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                ForeColor = StyleManager.AccentColor
            };
            panel.Controls.Add(title);

            // Панель кнопок отчетов
            var reportsPanel = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(panel.Width - 40, 400),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            // Кнопки отчетов
            int y = 0;
            string[] reports = {
        "📊 Отчет по продажам",
        "🏭 Отчет по производству",
        "📦 Анализ запасов материалов",
        "💰 Финансовый отчет",
        "👥 Отчет по персоналу",
        "🤝 Отчет по партнерам",
        "⏱️ Отчет по срокам производства"
    };

            // Ширина кнопок - занимают всю ширину панели минус небольшие отступы
            int buttonWidth = reportsPanel.Width - 10; // Почти вся ширина

            foreach (var report in reports)
            {
                var btnReport = new Button
                {
                    Text = report,
                    Location = new Point(5, y), // 5px отступ слева
                    Size = new Size(buttonWidth, 45),
                    Font = new Font("Comic Sans MS", 11),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag = report
                };
                StyleManager.ApplyButtonStyle(btnReport);

                btnReport.Click += (s, e) => GenerateReport(btnReport.Text);

                reportsPanel.Controls.Add(btnReport);
                y += 50;
            }

            // Подписываемся на изменение размера, чтобы кнопки всегда были во всю ширину
            reportsPanel.SizeChanged += (s, e) =>
            {
                int width = reportsPanel.Width - 10;
                foreach (Control control in reportsPanel.Controls)
                {
                    if (control is Button button)
                    {
                        button.Width = width;
                    }
                }
            };

            reportsPanel.Height = y + 20;
            panel.Controls.Add(reportsPanel);

            return panel;
        }

        /// <summary>
        /// Генерация отчета
        /// </summary>
        private void GenerateReport(string reportName)
        {
            try
            {
                // Показываем прогресс
                var progressForm = new Form
                {
                    Text = "Генерация отчета",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog
                };

                var lblProgress = new Label
                {
                    Text = $"Генерация отчета: {reportName}...",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Comic Sans MS", 10)
                };

                var progressBar = new ProgressBar
                {
                    Location = new Point(20, 60),
                    Size = new Size(340, 30),
                    Style = ProgressBarStyle.Marquee
                };

                progressForm.Controls.Add(lblProgress);
                progressForm.Controls.Add(progressBar);

                // Запускаем в отдельном потоке для имитации генерации
                Task.Run(() =>
                {
                    // Имитируем задержку генерации отчета
                    Thread.Sleep(1500);

                    // В основном потоке показываем результат
                    progressForm.Invoke((MethodInvoker)delegate
                    {
                        progressForm.Close();
                        ShowReportResult(reportName);
                    });
                });

                progressForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Показ результата отчета
        /// </summary>
        private void ShowReportResult(string reportName)
        {
            // Создаем форму с отчетом
            var reportForm = new Form
            {
                Text = $"Отчет: {reportName}",
                Size = new Size(900, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(700, 400)
            };
            StyleManager.ApplyFormStyle(reportForm);

            // Заголовок
            var title = new Label
            {
                Text = reportName,
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            // Дата генерации
            var lblDate = new Label
            {
                Text = $"Сгенерирован: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 9),
                ForeColor = Color.Gray
            };

            // Таблица с данными отчета
            var grid = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(reportForm.ClientSize.Width - 40, reportForm.ClientSize.Height - 180),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };
            StyleManager.ApplyGridStyle(grid);

            // Загружаем данные отчета
            LoadReportData(grid, reportName);

            // Панель кнопок
            var buttonsPanel = new Panel
            {
                Location = new Point(20, reportForm.ClientSize.Height - 70),
                Size = new Size(reportForm.ClientSize.Width - 40, 40),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var btnExportExcel = new Button
            {
                Text = "💾 Экспорт в Excel",
                Location = new Point(0, 0),
                Size = new Size(150, 35)
            };
            StyleManager.ApplyButtonStyle(btnExportExcel, true);
            btnExportExcel.Click += (s, e) => ExportToExcel(reportName, grid);

            // Кнопка экспорта в Word - тот же цвет, что и у Excel
            var btnExportWord = new Button
            {
                Text = "📝 Экспорт в Word",
                Location = new Point(160, 0), // Позиция X = 160 (Excel занимает 150)
                Size = new Size(150, 35)
            };
            StyleManager.ApplyButtonStyle(btnExportWord, true); // true = акцентный цвет (как у Excel)
            btnExportWord.Click += (s, e) => ExportToWord(reportName, grid);

            // Кнопка закрыть - с отступом 5px от Word
            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(315, 0), // 160 + 150 + 5 = 315 (отступ 5px от Word)
                Size = new Size(120, 35)
            };
            StyleManager.ApplyButtonStyle(btnClose);
            btnClose.Click += (s, e) => reportForm.Close();

            buttonsPanel.Controls.Add(btnExportExcel);
            buttonsPanel.Controls.Add(btnExportWord);
            buttonsPanel.Controls.Add(btnClose);

            reportForm.Controls.Add(title);
            reportForm.Controls.Add(lblDate);
            reportForm.Controls.Add(grid);
            reportForm.Controls.Add(buttonsPanel);

            reportForm.Show();
        }

        /// <summary>
        /// Экспорт в Word (имитация)
        /// </summary>
        /// <summary>
        /// Экспорт в Word (реальный RTF-файл)
        /// </summary>
        /// <summary>
        /// Экспорт в Word (простой RTF)
        /// </summary>
        /// <summary>
        /// Экспорт в Word (исправленная кодировка)
        /// </summary>
        private void ExportToWord(string reportName, DataGridView grid)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить отчет в Word",
                    Filter = "Документы Word (*.doc)|*.doc|RTF документы (*.rtf)|*.rtf|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    FileName = $"{CleanReportName(reportName)}_{DateTime.Now:yyyyMMdd_HHmm}",
                    DefaultExt = ".doc"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Создаем RTF документ с правильной кодировкой
                    string rtfContent = CreateRtfWithRussianText(reportName, grid);

                    // Сохраняем с кодировкой Windows-1251 (для русских символов)
                    File.WriteAllText(saveDialog.FileName, rtfContent, Encoding.GetEncoding(1251));

                    MessageBox.Show($"Отчет успешно сохранен!\n\n" +
                                  $"Файл: {saveDialog.FileName}\n" +
                                  $"Формат: RTF/DOC\n" +
                                  $"Размер: {new FileInfo(saveDialog.FileName).Length / 1024} КБ\n\n" +
                                  $"Рекомендация: откройте в Microsoft Word",
                        "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}\n\n" +
                               $"Попробуйте экспорт в Excel.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Очистка названия отчета от эмодзи
        /// </summary>
        private string CleanReportName(string reportName)
        {
            return reportName.Replace("📊 ", "").Replace("🏭 ", "").Replace("📦 ", "")
                            .Replace("💰 ", "").Replace("👥 ", "").Replace("🤝 ", "")
                            .Replace("⏱️ ", "");
        }

        /// <summary>
        /// Создание RTF с поддержкой русского текста
        /// </summary>
        private string CreateRtfWithRussianText(string reportName, DataGridView grid)
        {
            StringBuilder rtf = new StringBuilder();

            // RTF заголовок с русской кодировкой
            rtf.Append(@"{\rtf1\ansi\ansicpg1251\deff0\nouicompat\deflang1049{\fonttbl{\f0\fnil\fcharset204 Calibri;}{\f1\fnil\fcharset0 Calibri;}}");
            rtf.Append(@"\viewkind4\uc1 ");
            rtf.Append(@"\pard\sa200\sl276\slmult1\f0\fs24\lang1049 ");

            // Заголовок отчета
            string cleanReportName = CleanReportName(reportName);
            rtf.Append(@"\b\fs32 " + cleanReportName + @"\b0\fs24\par\par");
            rtf.Append(@"Дата генерации: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + @"\par\par");
            rtf.Append(@"\pard\qc\line\par\par");

            // Таблица
            rtf.Append(@"\b\fs20 ");
            foreach (DataGridViewColumn column in grid.Columns)
            {
                rtf.Append(EscapeRtfText(column.HeaderText) + @"\tab ");
            }
            rtf.Append(@"\b0\fs24\par");

            // Разделитель
            rtf.Append(@"\pard\brdrt\brdrs\brdrw10\brsp20 \par\par");

            // Данные таблицы
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (!row.IsNewRow)
                {
                    for (int i = 0; i < grid.Columns.Count; i++)
                    {
                        string cellValue = row.Cells[i].Value?.ToString() ?? "";
                        rtf.Append(EscapeRtfText(cellValue) + @"\tab ");
                    }
                    rtf.Append(@"\par");
                }
            }

            // Подвал
            rtf.Append(@"\par\par");
            rtf.Append(@"\pard\sa200\sl276\slmult1\f1\fs18\lang9 ");
            rtf.Append(@"\i Отчет сгенерирован автоматически в системе АСУП 'Керамик-Про'\i0\par");

            rtf.Append(@"}");

            return rtf.ToString();
        }

        /// <summary>
        /// Экранирование специальных символов для RTF
        /// </summary>
        private string EscapeRtfText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Заменяем специальные символы RTF
            return text.Replace("\\", "\\\\")
                       .Replace("{", "\\{")
                       .Replace("}", "\\}")
                       .Replace("\r\n", "\\par ")
                       .Replace("\n", "\\par ")
                       .Replace("\r", "\\par ")
                       .Replace("\t", "\\tab ");
        }

        /// <summary>
        /// Загрузка данных для отчета
        /// </summary>
        private void LoadReportData(DataGridView grid, string reportName)
        {
            DataTable data = new DataTable();

            switch (reportName)
            {
                case "📊 Отчет по продажам":
                    data = CreateSalesReport();
                    break;
                case "🏭 Отчет по производству":
                    data = CreateProductionReport();
                    break;
                case "📦 Анализ запасов материалов":
                    data = CreateInventoryReport();
                    break;
                case "💰 Финансовый отчет":
                    data = CreateFinancialReport();
                    break;
                case "👥 Отчет по персоналу":
                    data = CreateStaffReport();
                    break;
                case "🤝 Отчет по партнерам":
                    data = CreatePartnersReport();
                    break;
                case "⏱️ Отчет по срокам производства":
                    data = CreateTimelineReport();
                    break;
            }

            grid.DataSource = data;
            AdjustGridColumns(grid);
        }

        /// <summary>
        /// Создание отчета по продажам
        /// </summary>
        private DataTable CreateSalesReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Месяц", typeof(string));
            dt.Columns.Add("Количество заказов", typeof(int));
            dt.Columns.Add("Общая сумма", typeof(decimal));
            dt.Columns.Add("Средний чек", typeof(decimal));
            dt.Columns.Add("Прирост", typeof(string));

            // Тестовые данные
            var months = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь" };
            var random = new Random();

            decimal prevSum = 0;
            foreach (var month in months)
            {
                int orders = random.Next(50, 200);
                decimal sum = orders * random.Next(500, 5000);
                decimal avg = sum / orders;

                string growth = "";
                if (prevSum > 0)
                {
                    decimal growthPercent = ((sum - prevSum) / prevSum) * 100;
                    growth = $"{growthPercent:F1}%";
                }

                dt.Rows.Add(month, orders, sum.ToString("N2"), avg.ToString("N2"), growth);
                prevSum = sum;
            }

            return dt;
        }

        /// <summary>
        /// Создание отчета по производству
        /// </summary>
        private DataTable CreateProductionReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Цех", typeof(string));
            dt.Columns.Add("Продукция", typeof(string));
            dt.Columns.Add("План", typeof(int));
            dt.Columns.Add("Факт", typeof(int));
            dt.Columns.Add("Выполнение", typeof(string));
            dt.Columns.Add("Брак", typeof(string));

            dt.Rows.Add("Цех №1", "Плитка керамическая", 10000, 9800, "98%", "2.1%");
            dt.Rows.Add("Цех №1", "Мозаика", 5000, 5100, "102%", "1.8%");
            dt.Rows.Add("Цех №2", "Керамогранит", 8000, 7600, "95%", "2.5%");
            dt.Rows.Add("Цех №2", "Декоративные элементы", 2000, 2100, "105%", "3.0%");

            return dt;
        }

        // Аналогичные методы для других отчетов...

        /// <summary>
        /// Экспорт отчета в Excel
        /// </summary>
        /// <summary>
        /// Экспорт отчета в Excel
        /// </summary>
        private void ExportToExcel(string reportName, DataGridView grid)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить отчет в Excel",
                    Filter = "Файлы Excel (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    FileName = $"{reportName.Replace("📊 ", "").Replace("🏭 ", "").Replace("📦 ", "").Replace("💰 ", "").Replace("👥 ", "").Replace("🤝 ", "").Replace("⏱️ ", "").Replace("📈 ", "")}_{DateTime.Now:yyyyMMdd_HHmm}",
                    DefaultExt = ".csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        // Заголовки
                        var headers = new List<string>();
                        foreach (DataGridViewColumn column in grid.Columns)
                        {
                            headers.Add(column.HeaderText);
                        }
                        writer.WriteLine(string.Join(";", headers));

                        // Данные
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var values = new List<string>();
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    values.Add(cell.Value?.ToString() ?? "");
                                }
                                writer.WriteLine(string.Join(";", values));
                            }
                        }
                    }

                    MessageBox.Show($"Отчет экспортирован в файл:\n{saveDialog.FileName}",
                        "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создание анализа запасов материалов
        /// </summary>
        private DataTable CreateInventoryReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Материал", typeof(string));
            dt.Columns.Add("Текущий остаток", typeof(int));
            dt.Columns.Add("Мин. запас", typeof(int));
            dt.Columns.Add("Разница", typeof(int));
            dt.Columns.Add("Статус", typeof(string));
            dt.Columns.Add("Рекомендация", typeof(string));

            dt.Rows.Add("Глина белая", 1200, 500, 700, "✅ Норма", "Заказ не требуется");
            dt.Rows.Add("Песок кварцевый", 2500, 1000, 1500, "✅ Норма", "Заказ не требуется");
            dt.Rows.Add("Глазурь синяя", 150, 50, 100, "✅ Норма", "Заказ не требуется");
            dt.Rows.Add("Краситель золотой", 5, 20, -15, "⚠️ Низкий запас", "Срочный заказ!");
            dt.Rows.Add("Упаковка картонная", 800, 1000, -200, "⚠️ Низкий запас", "Заказать 500 шт.");

            return dt;
        }

        /// <summary>
        /// Создание финансового отчета
        /// </summary>
        private DataTable CreateFinancialReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Статья", typeof(string));
            dt.Columns.Add("Январь", typeof(decimal));
            dt.Columns.Add("Февраль", typeof(decimal));
            dt.Columns.Add("Март", typeof(decimal));
            dt.Columns.Add("Итого", typeof(decimal));

            dt.Rows.Add("Выручка", 1250000.50m, 1340000.00m, 1420000.75m, 4010001.25m);
            dt.Rows.Add("Себестоимость", 750000.30m, 800000.00m, 850000.25m, 2400000.55m);
            dt.Rows.Add("Прибыль", 500000.20m, 540000.00m, 570000.50m, 1610000.70m);
            dt.Rows.Add("Расходы", 200000.00m, 210000.00m, 220000.00m, 630000.00m);
            dt.Rows.Add("Чистая прибыль", 300000.20m, 330000.00m, 350000.50m, 980000.70m);

            return dt;
        }

        /// <summary>
        /// Создание отчета по персоналу
        /// </summary>
        private DataTable CreateStaffReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Отдел", typeof(string));
            dt.Columns.Add("Сотрудников", typeof(int));
            dt.Columns.Add("Средняя з/п", typeof(decimal));
            dt.Columns.Add("Текучесть", typeof(string));
            dt.Columns.Add("Вакансии", typeof(int));

            dt.Rows.Add("Производство", 45, 45000.00m, "3.2%", 2);
            dt.Rows.Add("Отдел продаж", 12, 65000.00m, "5.1%", 1);
            dt.Rows.Add("Бухгалтерия", 5, 55000.00m, "0%", 0);
            dt.Rows.Add("IT-отдел", 8, 75000.00m, "2.5%", 1);
            dt.Rows.Add("Склад", 15, 40000.00m, "4.8%", 0);

            return dt;
        }

        /// <summary>
        /// Создание отчета по партнерам
        /// </summary>
        private DataTable CreatePartnersReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Партнер", typeof(string));
            dt.Columns.Add("Тип", typeof(string));
            dt.Columns.Add("Кол-во заказов", typeof(int));
            dt.Columns.Add("Общая сумма", typeof(decimal));
            dt.Columns.Add("Средний чек", typeof(decimal));
            dt.Columns.Add("Рейтинг", typeof(string));

            dt.Rows.Add("ООО 'СтройТорг'", "Оптовый", 23, 1250000.50m, 54347.85m, "4.8 ⭐");
            dt.Rows.Add("АО 'Керамика Плюс'", "Розничный", 45, 890000.00m, 19777.78m, "4.5 ⭐");
            dt.Rows.Add("ИП 'Мастерская отделки'", "Монтажник", 12, 456000.00m, 38000.00m, "4.9 ⭐");
            dt.Rows.Add("ООО 'ДомСтрой'", "Оптовый", 18, 2345000.00m, 130277.78m, "4.7 ⭐");

            return dt;
        }

        /// <summary>
        /// Создание отчета по срокам производства
        /// </summary>
        private DataTable CreateTimelineReport()
        {
            var dt = new DataTable();
            dt.Columns.Add("Заказ", typeof(string));
            dt.Columns.Add("Продукция", typeof(string));
            dt.Columns.Add("Плановый срок", typeof(string));
            dt.Columns.Add("Фактический срок", typeof(string));
            dt.Columns.Add("Отклонение", typeof(string));
            dt.Columns.Add("Статус", typeof(string));

            dt.Rows.Add("№00123", "Плитка 30x30", "15.12.2023", "14.12.2023", "-1 день", "✅ Досрочно");
            dt.Rows.Add("№00124", "Мозаика", "18.12.2023", "18.12.2023", "0 дней", "✅ В срок");
            dt.Rows.Add("№00125", "Керамогранит", "20.12.2023", "22.12.2023", "+2 дня", "⚠️ Задержка");
            dt.Rows.Add("№00126", "Декоративные элементы", "25.12.2023", "24.12.2023", "-1 день", "✅ Досрочно");

            return dt;
        }

        private void CreateTestData(DataGridView grid, string module)
        {
            var dt = new DataTable();

            switch (module)
            {
                case "Партнеры":
                    dt.Columns.Add("ID");
                    dt.Columns.Add("Компания");
                    dt.Columns.Add("Тип");
                    dt.Columns.Add("Рейтинг", typeof(decimal));
                    dt.Columns.Add("Телефон");
                    dt.Columns.Add("Email");
                    dt.Columns.Add("Дата регистрации");
                    dt.Rows.Add(1, "ООО 'СтройТорг'", "Оптовый", 4.8, "+7(999)123-45-67", "info@stroitorg.ru", "15.12.2023");
                    dt.Rows.Add(2, "АО 'Керамика Плюс'", "Розничный", 4.5, "+7(999)765-43-21", "sales@keramika.ru", "16.12.2023");
                    break;

                case "Заявки":
                    dt.Columns.Add("№ заявки");
                    dt.Columns.Add("Партнер");
                    dt.Columns.Add("Дата создания");
                    dt.Columns.Add("Сумма", typeof(decimal));
                    dt.Columns.Add("Статус");
                    dt.Columns.Add("Менеджер");
                    dt.Rows.Add("00123", "ООО 'СтройТорг'", "15.12.2023", 125000.50, "НОВАЯ", "Иванов И.И.");
                    dt.Rows.Add("00124", "АО 'Керамика Плюс'", "16.12.2023", 89000.00, "В ПРОИЗВОДСТВЕ", "Петров П.П.");
                    break;

                case "Производство":
                    dt.Columns.Add("ID задания", typeof(int));
                    dt.Columns.Add("ID заявки", typeof(int));
                    dt.Columns.Add("Мастер", typeof(string));
                    dt.Columns.Add("Дата создания", typeof(string));
                    dt.Columns.Add("Срок выполнения", typeof(string));
                    dt.Columns.Add("Статус", typeof(string));
                    dt.Columns.Add("Приоритет", typeof(int));

                    dt.Rows.Add(1, 123, "Иванов И.И.", "15.12.2023 10:30", "20.12.2023", "В работе", 1);
                    dt.Rows.Add(2, 124, "Петров П.П.", "16.12.2023 09:15", "18.12.2023", "Завершено", 2);
                    dt.Rows.Add(3, 125, "Сидоров С.С.", "17.12.2023 14:00", "22.12.2023", "Назначено", 1);
                    break;

                default:
                    dt.Columns.Add("Наименование");
                    dt.Columns.Add("Код");
                    dt.Columns.Add("Описание");
                    dt.Rows.Add("Тестовый элемент 1", "001", "Описание тестового элемента");
                    dt.Rows.Add("Тестовый элемент 2", "002", "Описание тестового элемента");
                    break;
            }

            grid.DataSource = dt;
        }

        #endregion
    }
}