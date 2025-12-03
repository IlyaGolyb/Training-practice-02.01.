using System.Drawing;
using System.Windows.Forms;

namespace МозаикаERP.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Авторизация - АСУП Керамик-Про";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            StyleManager.ApplyFormStyle(this);

            // Заголовок
            var title = new Label
            {
                Text = "Вход в систему",
                Location = new Point(20, 20),
                Font = new Font("Comic Sans MS", 16, FontStyle.Bold),
                AutoSize = true,
                ForeColor = StyleManager.AccentColor
            };

            // Поля ввода
            int y = 70;

            var lblUsername = new Label { Text = "Логин:", Location = new Point(20, y) };
            var txtUsername = new TextBox
            {
                Location = new Point(120, y - 3),
                Size = new Size(200, 25),
                Name = "txtUsername"
            };
            y += 40;

            var lblPassword = new Label { Text = "Пароль:", Location = new Point(20, y) };
            var txtPassword = new TextBox
            {
                Location = new Point(120, y - 3),
                Size = new Size(200, 25),
                PasswordChar = '*',
                Name = "txtPassword"
            };
            y += 50;

            // Кнопка входа
            var btnLogin = new Button
            {
                Text = "🔑 Войти",
                Location = new Point(120, y),
                Size = new Size(150, 35)
            };
            StyleManager.ApplyButtonStyle(btnLogin, true);

            // Информация о тестовых пользователях
            var lblInfo = new Label
            {
                Text = "Тестовые пользователи:\nadmin/admin123 (полный доступ)\nmanager/manager123\nanalyst/analyst123\nwarehouse/warehouse123\nmaster/master123",
                Location = new Point(20, y + 50),
                AutoSize = true,
                Font = new Font("Comic Sans MS", 8),
                ForeColor = Color.Gray
            };

            btnLogin.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Введите логин и пароль!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (AuthService.Login(txtUsername.Text, txtPassword.Text))
                {
                    // Простое сообщение для ВСЕХ ролей
                    MessageBox.Show($"Добро пожаловать, {txtUsername.Text}!",
                        "Авторизация успешна", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Обработчик нажатия Enter
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnLogin.PerformClick();
                }
            };

            // Добавляем элементы на форму
            this.Controls.Add(title);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(lblInfo);

            // Применяем стили
            foreach (Control control in this.Controls)
            {
                if (control is Label label) StyleManager.ApplyLabelStyle(label);
                else if (control is TextBox textBox) StyleManager.ApplyTextBoxStyle(textBox);
            }
        }
    }
}
