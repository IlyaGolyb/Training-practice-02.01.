using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using МозаикаERP.Calculations;
using МозаикаERP.Services;

namespace МозаикаERP.Forms
{
    public partial class SuppliersForm : Form
    {
        public SuppliersForm()
        {
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Модуль 4 - Поставщики материалов и расчет продукции";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            StyleManager.ApplyFormStyle(this);

            // Создаем TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = StyleManager.DefaultFont,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 30)
            };

            // Вкладка 1: Поставщики
            var tabSuppliers = new TabPage("📊 Поставщики материалов");
            SetupSuppliersTab(tabSuppliers);

            // Вкладка 2: Расчет продукции
            var tabCalculation = new TabPage("🧮 Расчет продукции");
            SetupCalculationTab(tabCalculation);

            tabControl.TabPages.Add(tabSuppliers);
            tabControl.TabPages.Add(tabCalculation);

            this.Controls.Add(tabControl);
        }

        private void SetupSuppliersTab(TabPage tab)
        {
            tab.BackColor = StyleManager.PrimaryBackground;

            // Заголовок
            var title = new Label
            {
                Text = "Список поставщиков материала (Модуль 4)",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold)
            };
            StyleManager.ApplyLabelStyle(title, true);

            // Таблица поставщиков
            var grid = new DataGridView
            {
                Location = new Point(30, 80),
                Size = new Size(tab.Width + 450, 400),
                Name = "gridSuppliers",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };
            StyleManager.ApplyGridStyle(grid);

            // Фильтр по материалу
            var lblMaterial = new Label
            {
                Text = "Материал:",
                Location = new Point(30, 500),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            // Выпадающий список
            var cmbMaterial = new ComboBox
            {
                Location = new Point(140, 495),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = StyleManager.DefaultFont
            };

            // Заполняем материалами
            cmbMaterial.Items.Add("Все материалы");
            cmbMaterial.Items.Add("Глина");
            cmbMaterial.Items.Add("Песок");
            cmbMaterial.Items.Add("Глазурь");
            cmbMaterial.Items.Add("Красители");
            cmbMaterial.SelectedIndex = 0;

            StyleManager.ApplyLabelStyle(lblMaterial);
            StyleManager.ApplyComboBoxStyle(cmbMaterial);

            // Кнопка загрузки поставщиков
            var btnLoad = new Button
            {
                Text = "Загрузить поставщиков",
                Location = new Point(410, 495),
                Size = new Size(180, 32)
            };
            StyleManager.ApplyButtonStyle(btnLoad, true);
            btnLoad.Click += (s, e) => LoadSuppliersData(grid, cmbMaterial.SelectedIndex);

            // Кнопка теста БД
            var btnTestDB = new Button
            {
                Text = "Тест подключения к БД",
                Location = new Point(610, 495),
                Size = new Size(180, 32)
            };
            StyleManager.ApplyButtonStyle(btnTestDB);
            btnTestDB.Click += (s, e) => TestDatabaseConnection();

            tab.Controls.Add(title);
            tab.Controls.Add(grid);
            tab.Controls.Add(lblMaterial);
            tab.Controls.Add(cmbMaterial);
            tab.Controls.Add(btnLoad);
            tab.Controls.Add(btnTestDB);

            // Автозагрузка при открытии
            LoadSuppliersData(grid, 0);
        }

        private void SetupCalculationTab(TabPage tab)
        {
            tab.BackColor = StyleManager.PrimaryBackground;

            // Заголовок
            var title = new Label
            {
                Text = "Расчет количества продукции из сырья (Модуль 4)",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 14, FontStyle.Bold)
            };
            StyleManager.ApplyLabelStyle(title, true);

            // Группа ввода параметров
            var groupBox = new GroupBox
            {
                Text = "Параметры расчета",
                Location = new Point(30, 80),
                Size = new Size(500, 280),
                Font = new Font("Comic Sans MS", 11, FontStyle.Bold)
            };
            StyleManager.ApplyGroupBoxStyle(groupBox, true);

            // Поля ввода с увеличенными отступами
            int y = 40;
            AddInputField(groupBox, "Тип продукции (ID):", "numProductType", 180, y, 1);
            y += 50;
            AddInputField(groupBox, "Тип материала (ID):", "numMaterialType", 180, y, 1);
            y += 50;
            AddInputField(groupBox, "Количество сырья:", "numRawMaterial", 180, y, 1000);
            y += 50;
            AddInputField(groupBox, "Параметр 1:", "numParam1", 180, y, 2.5);
            y += 50;
            AddInputField(groupBox, "Параметр 2:", "numParam2", 180, y, 3.0);

            // Кнопка расчета
            var btnCalculate = new Button
            {
                Text = "Рассчитать количество продукции",
                Location = new Point(180, 380),
                Size = new Size(250, 45),
                Font = new Font("Comic Sans MS", 11)
            };
            StyleManager.ApplyButtonStyle(btnCalculate, true);
            btnCalculate.Click += BtnCalculate_Click;

            // Поле результата
            var lblResult = new Label
            {
                Name = "lblResult",
                Text = "Результат: ",
                Location = new Point(30, 440),
                AutoSize = true,
                Size = new Size(600, 50),
                Font = new Font("Comic Sans MS", 12)
            };
            StyleManager.ApplyLabelStyle(lblResult);

            // Поле с деталями расчета
            var txtDetails = new TextBox
            {
                Name = "txtDetails",
                Location = new Point(30, 490),
                Size = new Size(500, 100),
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Comic Sans MS", 10),
                ScrollBars = ScrollBars.Vertical
            };
            StyleManager.ApplyTextBoxStyle(txtDetails);

            tab.Controls.Add(title);
            tab.Controls.Add(groupBox);
            tab.Controls.Add(btnCalculate);
            tab.Controls.Add(lblResult);
            tab.Controls.Add(txtDetails);
        }

        private void AddInputField(Control parent, string labelText, string name, int x, int y, object defaultValue)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(30, y),
                Size = new Size(140, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Font = StyleManager.DefaultFont
            };
            StyleManager.ApplyLabelStyle(lbl);

            NumericUpDown numericUpDown;

            if (defaultValue is double)
            {
                numericUpDown = new NumericUpDown
                {
                    Name = name,
                    Location = new Point(x, y),
                    Size = new Size(150, 30),
                    DecimalPlaces = 2,
                    Minimum = 0.1m,
                    Maximum = 1000,
                    Value = (decimal)(double)defaultValue,
                    Font = StyleManager.DefaultFont
                };
            }
            else
            {
                numericUpDown = new NumericUpDown
                {
                    Name = name,
                    Location = new Point(x, y),
                    Size = new Size(150, 30),
                    Minimum = 1,
                    Maximum = 10000,
                    Value = (int)defaultValue,
                    Font = StyleManager.DefaultFont
                };
            }

            StyleManager.ApplyNumericUpDownStyle(numericUpDown);

            parent.Controls.Add(lbl);
            parent.Controls.Add(numericUpDown);
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                // Находим TabControl
                var tabControl = this.Controls[0] as TabControl;
                if (tabControl == null) return;

                var tabCalculation = tabControl.TabPages[1];

                // Вспомогательная функция поиска контролов
                Control FindControl(string name)
                {
                    foreach (Control control in tabCalculation.Controls)
                    {
                        var found = FindControlRecursive(control, name);
                        if (found != null) return found;
                    }
                    return null;
                }

                Control FindControlRecursive(Control parent, string name)
                {
                    if (parent.Name == name) return parent;

                    foreach (Control child in parent.Controls)
                    {
                        var found = FindControlRecursive(child, name);
                        if (found != null) return found;
                    }
                    return null;
                }

                // Получаем контролы
                var numProductType = FindControl("numProductType") as NumericUpDown;
                var numMaterialType = FindControl("numMaterialType") as NumericUpDown;
                var numRawMaterial = FindControl("numRawMaterial") as NumericUpDown;
                var numParam1 = FindControl("numParam1") as NumericUpDown;
                var numParam2 = FindControl("numParam2") as NumericUpDown;
                var lblResult = FindControl("lblResult") as Label;
                var txtDetails = FindControl("txtDetails") as TextBox;

                if (numProductType == null || numMaterialType == null ||
                    numRawMaterial == null || numParam1 == null || numParam2 == null ||
                    lblResult == null || txtDetails == null)
                {
                    MessageBox.Show("Не удалось найти элементы управления", "Ошибка");
                    return;
                }

                // Выполняем расчет
                int result = ProductionCalculator.CalculateProductQuantity(
                    (int)numProductType.Value,
                    (int)numMaterialType.Value,
                    (int)numRawMaterial.Value,
                    (double)numParam1.Value,
                    (double)numParam2.Value);

                // Отображаем результат
                if (result == -1)
                {
                    lblResult.Text = "❌ Ошибка расчета! Проверьте введенные данные.";
                    lblResult.ForeColor = Color.Red;
                    txtDetails.Text = "Возможные причины ошибки:\n" +
                                    "1. Несуществующий тип продукции\n" +
                                    "2. Несуществующий тип материала\n" +
                                    "3. Некорректные параметры (должны быть > 0)\n" +
                                    "4. Ошибка в расчетах";
                }
                else
                {
                    lblResult.Text = $"✅ Из {numRawMaterial.Value} ед. сырья можно произвести {result} ед. продукции";
                    lblResult.ForeColor = Color.Green;

                    // Детали расчета
                    txtDetails.Text = $"Детали расчета:\n" +
                                    $"• Тип продукции: {numProductType.Value}\n" +
                                    $"• Тип материала: {numMaterialType.Value}\n" +
                                    $"• Количество сырья: {numRawMaterial.Value} ед.\n" +
                                    $"• Параметр 1: {numParam1.Value:F2}\n" +
                                    $"• Параметр 2: {numParam2.Value:F2}\n" +
                                    $"• Результат: {result} ед. продукции\n\n" +
                                    $"Примечание: Расчет учитывает потери сырья при производстве.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliersData(DataGridView grid, int materialIndex)
        {
            try
            {
                DataTable dt;

                if (materialIndex == 0)
                {
                    // Все поставщики
                    dt = DatabaseService.GetSuppliers();
                }
                else
                {
                    // Поставщики для конкретного материала
                    dt = new DataTable();
                    dt.Columns.Add("Поставщик");
                    dt.Columns.Add("Тип поставщика");
                    dt.Columns.Add("Рейтинг", typeof(decimal));
                    dt.Columns.Add("Начало работы");
                    dt.Columns.Add("Кол-во материалов", typeof(int));

                    string material = "";
                    switch (materialIndex)
                    {
                        case 1: material = "Глина"; break;
                        case 2: material = "Песок"; break;
                        case 3: material = "Глазурь"; break;
                        case 4: material = "Красители"; break;
                    }

                    // Тестовые данные
                    dt.Rows.Add($"ООО 'Поставки {material}'", "Основной", 4.8, "15.03.2020", 12);
                    dt.Rows.Add($"АО '{material} Профи'", "Дополнительный", 4.5, "10.07.2021", 8);
                    dt.Rows.Add($"ИП 'Мастер {material}'", "Локальный", 4.2, "22.11.2022", 5);
                }

                grid.DataSource = dt;

                // Настраиваем ширину колонок
                if (grid.Columns.Count > 0)
                {
                    grid.Columns[0].Width = 250; // Название компании
                    grid.Columns[1].Width = 120; // Тип
                    grid.Columns[2].Width = 80;  // Рейтинг
                    grid.Columns[3].Width = 120; // Дата
                    if (grid.Columns.Count > 4)
                        grid.Columns[4].Width = 100; // Кол-во материалов
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\nПроверьте подключение к БД.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Тестовые данные при ошибке
                CreateTestData(grid);
            }
        }

        private void CreateTestData(DataGridView grid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Поставщик");
            dt.Columns.Add("Тип поставщика");
            dt.Columns.Add("Рейтинг", typeof(decimal));
            dt.Columns.Add("Начало работы");
            dt.Columns.Add("Кол-во материалов", typeof(int));

            dt.Rows.Add("ООО 'СтройМатериалы'", "Основной", 4.8, "15.03.2020", 12);
            dt.Rows.Add("АО 'Керамика Профи'", "Дополнительный", 4.5, "10.07.2021", 8);
            dt.Rows.Add("ИП Иванов И.И.", "Локальный", 4.2, "22.11.2022", 5);
            dt.Rows.Add("ЗАО 'ГлобалСнаб'", "Импортный", 4.9, "05.01.2019", 15);
            dt.Rows.Add("НПП 'ТехноКерамика'", "Основной", 4.7, "30.05.2020", 10);

            grid.DataSource = dt;
        }

        private void TestDatabaseConnection()
        {
            string result = DatabaseService.TestConnection();
            MessageBox.Show(result, "Тест подключения к БД");
        }
    }
}