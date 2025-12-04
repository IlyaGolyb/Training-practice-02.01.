using System;
using System.Drawing;
using System.Windows.Forms;
using ShoeStoreLLC.Data;
using ShoeStoreLLC.Models;
using System.Data.SqlClient;

namespace ShoeStoreLLC.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnGuest;
        private PictureBox logo;

        public User CurrentUser { get; private set; }

        public LoginForm()
        {
            // Установка иконки формы
            try
            {
                string iconPath = "Icon.ico";
                if (System.IO.File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else if (System.IO.File.Exists(System.IO.Path.Combine("Resources", "Icon.ico")))
                {
                    this.Icon = new Icon(System.IO.Path.Combine("Resources", "Icon.ico"));
                }
            }
            catch
            {
                // Если не удалось загрузить иконку, оставляем стандартную
            }

            SetupUI();
        }

        private void SetupUI()
        {
            // Настройка формы
            this.Text = "Вход в систему - ООО Обувь";
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Логотип - создаем простой PictureBox
            var logo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(150, 150),
                Location = new Point(125, 20)
            };

            // Загружаем логотип
            LoadLogoImage(logo);

            // Метки и поля ввода
            var lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(50, 200),
                Size = new Size(100, 25),
                Font = new Font("Times New Roman", 12)
            };

            txtLogin = new TextBox
            {
                Location = new Point(150, 200),
                Size = new Size(200, 25),
                Font = new Font("Times New Roman", 12)
            };

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 240),
                Size = new Size(100, 25),
                Font = new Font("Times New Roman", 12)
            };

            txtPassword = new TextBox
            {
                Location = new Point(150, 240),
                Size = new Size(200, 25),
                Font = new Font("Times New Roman", 12),
                PasswordChar = '*'
            };

            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(150, 290),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 12),
                BackColor = Color.FromArgb(0, 250, 154) // #00FA9A
            };

            btnGuest = new Button
            {
                Text = "Войти как гость",
                Location = new Point(100, 340),
                Size = new Size(200, 35),
                Font = new Font("Times New Roman", 12),
                BackColor = Color.FromArgb(127, 255, 0) // #7FFF00
            };

            // События
            btnLogin.Click += BtnLogin_Click;
            btnGuest.Click += BtnGuest_Click;

            // Добавление элементов
            this.Controls.AddRange(new Control[] { logo, lblLogin, txtLogin,
                lblPassword, txtPassword, btnLogin, btnGuest });
        }

        private void LoadLogoImage(PictureBox pictureBox)
        {
            try
            {
                string logoPath = "logo.png";

                // Ищем логотип в разных местах
                string[] possiblePaths = {
            logoPath,
            System.IO.Path.Combine("Resources", logoPath),
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logoPath),
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", logoPath)
        };

                foreach (string path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        pictureBox.Image = Image.FromFile(path);
                        return;
                    }
                }

                // Создаем простой логотип если файл не найден
                CreateDefaultLogo(pictureBox);
            }
            catch
            {
                CreateDefaultLogo(pictureBox);
            }
        }

        private void CreateDefaultLogo(PictureBox pictureBox)
        {
            var bmp = new Bitmap(150, 150);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawRectangle(Pens.Black, 0, 0, 149, 149);

                using (var font = new Font("Times New Roman", 16, FontStyle.Bold))
                {
                    g.DrawString("ООО", font, Brushes.Black, 40, 30);
                    g.DrawString("Обувь", font, Brushes.Black, 30, 70);
                }
            }
            pictureBox.Image = bmp;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            Authenticate(txtLogin.Text, txtPassword.Text);
        }

        private void BtnGuest_Click(object sender, EventArgs e)
        {
            EnterAsGuest();
        }

        private void Authenticate(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT u.UserID, u.FullName, u.Login, u.PasswordHash, 
                               u.RoleID, r.RoleName
                        FROM Users u
                        INNER JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE u.Login = @Login AND u.PasswordHash = @Password";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentUser = new User
                                {
                                    UserID = (int)reader["UserID"],
                                    FullName = reader["FullName"].ToString(),
                                    Login = reader["Login"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    RoleID = (int)reader["RoleID"],
                                    RoleName = reader["RoleName"].ToString()
                                };

                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnterAsGuest()
        {
            CurrentUser = null;
            this.DialogResult = DialogResult.Yes; // Отдельный код для гостя
            this.Close();
        }
    }
}