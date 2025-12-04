using System;
using System.Drawing;
using System.Windows.Forms;
using ShoeStoreLLC.Models;

namespace ShoeStoreLLC.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private Panel contentPanel;
        private Label lblUserInfo;

        public MainForm(User user)
        {
            currentUser = user;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "ООО Обувь - Магазин обуви";
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Верхняя панель
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(127, 255, 0) // #7FFF00
            };

            // Логотип слева
            var logo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(150, 150),
                Location = new Point(125, 20)
            };

            // Загружаем логотип
            LoadLogoImage(logo);

            // Информация о пользователе
            string userText;
            if (currentUser != null)
            {
                userText = $"Пользователь: {currentUser.FullName} ({currentUser.RoleName})";
            }
            else
            {
                userText = "Гость";
            }

            lblUserInfo = new Label
            {
                Text = userText,
                Location = new Point(60, 15),
                Size = new Size(400, 20),
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                ForeColor = Color.Black
            };

            // Кнопка выхода
            var btnLogout = new Button
            {
                Text = "Выйти",
                Size = new Size(100, 30),
                Location = new Point(this.ClientSize.Width - 120, 10),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.FromArgb(0, 250, 154) // #00FA9A
            };

            btnLogout.Click += BtnLogout_Click;

            topPanel.Controls.Add(logo);
            topPanel.Controls.Add(lblUserInfo);
            topPanel.Controls.Add(btnLogout);

            // Панель меню (слева)
            var menuPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Кнопка товаров
            var btnProducts = new Button
            {
                Text = "Товары",
                Size = new Size(180, 40),
                Location = new Point(10, 20),
                Font = new Font("Times New Roman", 12),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnProducts.Click += (s, e) => ShowProducts();

            menuPanel.Controls.Add(btnProducts);

            // Дополнительные кнопки для администратора - заменяем switch на if
            if (currentUser != null && currentUser.RoleID == UserRoles.Admin)
            {
                var btnEditProducts = new Button
                {
                    Text = "Управление товарами",
                    Size = new Size(180, 40),
                    Location = new Point(10, 70),
                    Font = new Font("Times New Roman", 12),
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                var btnOrders = new Button
                {
                    Text = "Заказы",
                    Size = new Size(180, 40),
                    Location = new Point(10, 120),
                    Font = new Font("Times New Roman", 12),
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                menuPanel.Controls.Add(btnEditProducts);
                menuPanel.Controls.Add(btnOrders);
            }

            // Панель контента
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Добавляем все на форму
            this.Controls.Add(contentPanel);
            this.Controls.Add(menuPanel);
            this.Controls.Add(topPanel);

            // Показываем товары по умолчанию
            ShowProducts();
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

        private void ShowProducts()
        {
            // Очищаем контентную панель
            contentPanel.Controls.Clear();

            Form productsForm;

            if (currentUser == null)
            {
                productsForm = new GuestProductsForm();
            }
            else if (currentUser.RoleID == UserRoles.Admin)
            {
                productsForm = new AdminProductsForm(currentUser);
            }
            else if (currentUser.RoleID == UserRoles.Manager)
            {
                productsForm = new ManagerProductsForm(currentUser);
            }
            else // Client
            {
                productsForm = new ClientProductsForm(currentUser);
            }

            // Настраиваем форму
            productsForm.TopLevel = false;
            productsForm.FormBorderStyle = FormBorderStyle.None;
            productsForm.Dock = DockStyle.Fill;

            // Добавляем в контентную панель
            contentPanel.Controls.Add(productsForm);
            productsForm.Show();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Выйти из учетной записи?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Возвращаемся к форме входа
                var loginForm = new LoginForm();
                this.Hide();

                var dialogResult = loginForm.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    currentUser = loginForm.CurrentUser;
                    lblUserInfo.Text = $"Пользователь: {currentUser.FullName} ({currentUser.RoleName})";
                    this.Show();
                    ShowProducts();
                }
                else if (dialogResult == DialogResult.Yes)
                {
                    currentUser = null;
                    lblUserInfo.Text = "Гость";
                    this.Show();
                    ShowProducts();
                }
                else
                {
                    Application.Exit();
                }
            }
        }
    }
}