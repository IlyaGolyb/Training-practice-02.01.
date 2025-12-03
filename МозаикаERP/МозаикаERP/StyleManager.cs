using System.Drawing;
using System.Windows.Forms;

namespace МозаикаERP
{
    public static class StyleManager
    {
        // Цвета из ТЗ
        public static Color PrimaryBackground => Color.White; // #FFFFFF
        public static Color SecondaryBackground => Color.FromArgb(0xAB, 0xCF, 0xCE); // #ABCFCE
        public static Color AccentColor => Color.FromArgb(0x54, 0x6F, 0x94); // #546F94
        public static Font DefaultFont => new Font("Comic Sans MS", 10);
        public static Font TitleFont => new Font("Comic Sans MS", 12, FontStyle.Bold);
        public static Font GridFont => new Font("Comic Sans MS", 9);

        /// <summary>
        /// Применяет стиль к форме
        /// </summary>
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = PrimaryBackground;
            form.Font = DefaultFont;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// Применяет стиль к панели
        /// </summary>
        public static void ApplyPanelStyle(Panel panel, bool isSecondary = false)
        {
            panel.BackColor = isSecondary ? SecondaryBackground : PrimaryBackground;
            panel.Font = DefaultFont;
        }

        /// <summary>
        /// Применяет стиль к GroupBox
        /// </summary>
        public static void ApplyGroupBoxStyle(GroupBox groupBox, bool isSecondary = false)
        {
            groupBox.BackColor = isSecondary ? SecondaryBackground : PrimaryBackground;
            groupBox.Font = DefaultFont;
            groupBox.ForeColor = Color.Black;
        }

        /// <summary>
        /// Применяет стиль к кнопке
        /// </summary>
        public static void ApplyButtonStyle(Button button, bool isAccent = false)
        {
            button.BackColor = isAccent ? AccentColor : PrimaryBackground;
            button.ForeColor = isAccent ? Color.White : Color.Black;
            button.Font = DefaultFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = AccentColor;
            button.FlatAppearance.BorderSize = 1;
            button.Size = new Size(150, 40);
        }

        /// <summary>
        /// Применяет стиль к DataGridView
        /// </summary>
        public static void ApplyGridStyle(DataGridView grid)
        {
            grid.BackgroundColor = SecondaryBackground;
            grid.GridColor = AccentColor;
            grid.DefaultCellStyle.BackColor = PrimaryBackground;
            grid.DefaultCellStyle.Font = GridFont;
            grid.DefaultCellStyle.ForeColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.BackColor = AccentColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = TitleFont;
            grid.EnableHeadersVisualStyles = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        /// <summary>
        /// Применяет стиль к TextBox
        /// </summary>
        public static void ApplyTextBoxStyle(TextBox textBox)
        {
            textBox.BackColor = PrimaryBackground;
            textBox.ForeColor = Color.Black;
            textBox.Font = DefaultFont;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Применяет стиль к ComboBox
        /// </summary>
        public static void ApplyComboBoxStyle(ComboBox comboBox)
        {
            comboBox.BackColor = PrimaryBackground;
            comboBox.ForeColor = Color.Black;
            comboBox.Font = DefaultFont;
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        /// <summary>
        /// Применяет стиль к NumericUpDown
        /// </summary>
        public static void ApplyNumericUpDownStyle(NumericUpDown numericUpDown)
        {
            numericUpDown.BackColor = PrimaryBackground;
            numericUpDown.ForeColor = Color.Black;
            numericUpDown.Font = DefaultFont;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Применяет стиль к Label
        /// </summary>
        public static void ApplyLabelStyle(Label label, bool isTitle = false)
        {
            label.Font = isTitle ? TitleFont : DefaultFont;
            label.ForeColor = Color.Black;
            label.BackColor = Color.Transparent;
        }

        /// <summary>
        /// Создает стилизованную кнопку навигации
        /// </summary>
        public static Button CreateNavButton(string text, Point location)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(180, 45),
                Tag = text // Храним название модуля в Tag
            };
            ApplyButtonStyle(button);
            return button;
        }

        /// <summary>
        /// Создает стилизованную панель модуля
        /// </summary>
        public static Panel CreateModulePanel(string moduleName)
        {
            var panel = new Panel
            {
                Name = $"panel{moduleName.Replace(" ", "")}",
                BackColor = PrimaryBackground,
                Dock = DockStyle.Fill,
                Visible = false,
                AutoScroll = true
            };

            // Заголовок модуля
            var titleLabel = new Label
            {
                Text = moduleName,
                Location = new Point(20, 20),
                AutoSize = true
            };
            ApplyLabelStyle(titleLabel, true);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        /// <summary>
        /// Создает стилизованную форму
        /// </summary>
        public static Form CreateStyledForm(string title, Size size)
        {
            var form = new Form
            {
                Text = title,
                Size = size,
                StartPosition = FormStartPosition.CenterParent
            };
            ApplyFormStyle(form);
            return form;
        }
    }
}