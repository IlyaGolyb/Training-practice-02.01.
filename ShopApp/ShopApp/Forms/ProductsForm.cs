using ShoeStoreLLC.Data;
using ShoeStoreLLC.Models;
using ShopApp.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ShoeStoreLLC.Forms
{
    public partial class ProductsForm : Form
    {
        protected User currentUser;
        protected Panel mainPanel;
        protected FlowLayoutPanel productsFlowPanel;

        public ProductsForm(User user = null)
        {
            currentUser = user;
            SetupUI();
            LoadProducts();
        }

        protected virtual void SetupUI()
        {
            // Настройка формы
            this.Text = "Список товаров - ООО Обувь";
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Times New Roman", 10);

            // Основная панель с прокруткой
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            // Панель для размещения карточек товаров
            productsFlowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            mainPanel.Controls.Add(productsFlowPanel);
            this.Controls.Add(mainPanel);
        }

        protected virtual void LoadProducts()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT p.*, 
                               s1.SupplierName, 
                               s2.SupplierName as ManufacturerName,
                               c.CategoryName
                        FROM Products p
                        LEFT JOIN Suppliers s1 ON p.SupplierID = s1.SupplierID
                        LEFT JOIN Suppliers s2 ON p.ManufacturerID = s2.SupplierID
                        LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                        ORDER BY p.ProductName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var products = new List<Product>();

                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ProductID = (int)reader["ProductID"],
                                ArticleNumber = reader["ArticleNumber"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                Price = (decimal)reader["Price"],
                                SupplierID = (int)reader["SupplierID"],
                                ManufacturerID = (int)reader["ManufacturerID"],
                                CategoryID = (int)reader["CategoryID"],
                                CurrentDiscount = (decimal)reader["CurrentDiscount"],
                                StockQuantity = (int)reader["StockQuantity"],
                                Description = reader["Description"].ToString(),
                                PhotoPath = reader["PhotoPath"] != DBNull.Value ? reader["PhotoPath"].ToString() : null,
                                SupplierName = reader["SupplierName"].ToString(),
                                ManufacturerName = reader["ManufacturerName"].ToString(),
                                CategoryName = reader["CategoryName"].ToString()
                            };

                            products.Add(product);
                        }

                        // Отображение товаров
                        DisplayProducts(products);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual void DisplayProducts(List<Product> products)
        {
            productsFlowPanel.SuspendLayout();
            productsFlowPanel.Controls.Clear();

            foreach (var product in products)
            {
                // Создаем панель для товара
                var productPanel = CreateProductPanel(product);
                productsFlowPanel.Controls.Add(productPanel);
            }

            productsFlowPanel.ResumeLayout();
        }

        protected Panel CreateProductPanel(Product product)
        {
            // Основная панель товара
            var productPanel = new Panel
            {
                Size = new Size(900, 200),
                Margin = new Padding(0, 0, 0, 20),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = GetProductBackgroundColor(product)
            };

            // === ЛЕВАЯ ЧАСТЬ - ФОТО ===
            var photoPanel = new Panel
            {
                Size = new Size(180, 180),
                Location = new Point(10, 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var pictureBox = new PictureBox
            {
                Size = new Size(170, 170),
                Location = new Point(5, 5),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            // Загружаем фото или используем заглушку
            if (!string.IsNullOrEmpty(product.PhotoPath) && System.IO.File.Exists(product.PhotoPath))
            {
                try
                {
                    pictureBox.Image = Image.FromFile(product.PhotoPath);
                }
                catch
                {
                    SetDefaultImage(pictureBox);
                }
            }
            else
            {
                SetDefaultImage(pictureBox);
            }

            photoPanel.Controls.Add(pictureBox);

            // === ПРАВАЯ ЧАСТЬ - ИНФОРМАЦИЯ ===
            var infoPanel = new Panel
            {
                Size = new Size(690, 180),
                Location = new Point(200, 10)
            };

            // 1. Заголовок с категорией и названием
            var titlePanel = new Panel
            {
                Size = new Size(680, 40),
                Location = new Point(0, 0)
            };

            var lblCategory = new Label
            {
                Text = $"Категория товара: {product.CategoryName}",
                Location = new Point(0, 0),
                Size = new Size(300, 20),
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                ForeColor = Color.Black
            };

            var lblProductName = new Label
            {
                Text = $"Наименование товара: {product.ProductName}",
                Location = new Point(0, 20),
                Size = new Size(400, 20),
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                ForeColor = Color.Black
            };

            titlePanel.Controls.Add(lblCategory);
            titlePanel.Controls.Add(lblProductName);

            // 2. Описание товара
            var lblDescription = new Label
            {
                Text = $"Описание товара: {product.Description}",
                Location = new Point(0, 45),
                Size = new Size(680, 20),
                Font = new Font("Times New Roman", 10),
                ForeColor = Color.Black
            };

            // 3. Производитель и поставщик
            var lblManufacturer = new Label
            {
                Text = $"Производитель: {product.ManufacturerName}",
                Location = new Point(0, 70),
                Size = new Size(340, 20),
                Font = new Font("Times New Roman", 10),
                ForeColor = Color.Black
            };

            var lblSupplier = new Label
            {
                Text = $"Поставщик: {product.SupplierName}",
                Location = new Point(340, 70),
                Size = new Size(340, 20),
                Font = new Font("Times New Roman", 10),
                ForeColor = Color.Black
            };

            // 4. Цена и единица измерения
            var pricePanel = new Panel
            {
                Location = new Point(0, 95),
                Size = new Size(340, 30)
            };

            if (product.CurrentDiscount > 0)
            {
                // Старая цена (перечеркнутая, красная)
                var lblOldPrice = new Label
                {
                    Text = $"{product.Price:N0} ₽",
                    Location = new Point(0, 0),
                    Size = new Size(100, 25),
                    Font = new Font("Times New Roman", 11, FontStyle.Strikeout),
                    ForeColor = Color.Red
                };

                // Новая цена (жирная, черная)
                var lblNewPrice = new Label
                {
                    Text = $"{product.FinalPrice:N0} ₽",
                    Location = new Point(110, 0),
                    Size = new Size(100, 25),
                    Font = new Font("Times New Roman", 11, FontStyle.Bold),
                    ForeColor = Color.Black
                };

                pricePanel.Controls.Add(lblOldPrice);
                pricePanel.Controls.Add(lblNewPrice);
            }
            else
            {
                // Обычная цена
                var lblPrice = new Label
                {
                    Text = $"Цена: {product.Price:N0} ₽",
                    Location = new Point(0, 0),
                    Size = new Size(200, 25),
                    Font = new Font("Times New Roman", 11),
                    ForeColor = Color.Black
                };

                pricePanel.Controls.Add(lblPrice);
            }

            var lblUnit = new Label
            {
                Text = $"Единица измерения: {product.UnitOfMeasure}",
                Location = new Point(340, 95),
                Size = new Size(200, 25),
                Font = new Font("Times New Roman", 10),
                ForeColor = Color.Black
            };

            // 5. Количество на складе
            var stockPanel = new Panel
            {
                Location = new Point(0, 130),
                Size = new Size(340, 25)
            };

            var lblStockText = new Label
            {
                Text = "Количество на складе:",
                Location = new Point(0, 0),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 10),
                ForeColor = Color.Black
            };

            var lblStockQuantity = new Label
            {
                Text = $"{product.StockQuantity}",
                Location = new Point(160, 0),
                Size = new Size(50, 25),
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                ForeColor = product.StockQuantity == 0 ? Color.Red : Color.Black
            };

            stockPanel.Controls.Add(lblStockText);
            stockPanel.Controls.Add(lblStockQuantity);

            // 6. Скидка
            var discountPanel = new Panel
            {
                Location = new Point(540, 130),
                Size = new Size(140, 40),
                BackColor = product.CurrentDiscount > 15 ? Color.FromArgb(46, 139, 87) : Color.Transparent
            };

            var lblDiscount = new Label
            {
                Text = $"Действующая скидка:",
                Location = new Point(0, 0),
                Size = new Size(140, 20),
                Font = new Font("Times New Roman", 9),
                ForeColor = product.CurrentDiscount > 15 ? Color.White : Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblDiscountValue = new Label
            {
                Text = $"{product.CurrentDiscount}%",
                Location = new Point(0, 20),
                Size = new Size(140, 20),
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                ForeColor = product.CurrentDiscount > 15 ? Color.White : Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };

            discountPanel.Controls.Add(lblDiscount);
            discountPanel.Controls.Add(lblDiscountValue);

            // Добавляем все элементы в infoPanel
            infoPanel.Controls.Add(titlePanel);
            infoPanel.Controls.Add(lblDescription);
            infoPanel.Controls.Add(lblManufacturer);
            infoPanel.Controls.Add(lblSupplier);
            infoPanel.Controls.Add(pricePanel);
            infoPanel.Controls.Add(lblUnit);
            infoPanel.Controls.Add(stockPanel);
            infoPanel.Controls.Add(discountPanel);

            // Добавляем панели в основную панель товара
            productPanel.Controls.Add(photoPanel);
            productPanel.Controls.Add(infoPanel);

            return productPanel;
        }

        private void SetDefaultImage(PictureBox pictureBox)
        {
            // Создаем простую заглушку
            var bmp = new Bitmap(170, 170);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawRectangle(Pens.Black, 0, 0, 169, 169);
                g.DrawString("Нет фото", new Font("Times New Roman", 12), Brushes.Black, 50, 75);
            }
            pictureBox.Image = bmp;
        }

        private Color GetProductBackgroundColor(Product product)
        {
            if (product.CurrentDiscount > 15)
            {
                return Color.FromArgb(46, 139, 87); // #2E8B57 для скидки >15%
            }
            else if (product.StockQuantity == 0)
            {
                return Color.LightCyan; // Голубой для отсутствующего товара
            }
            else
            {
                return Color.White;
            }
        }
    }

    // Форма для гостя
    public class GuestProductsForm : ProductsForm
    {
        public GuestProductsForm() : base(null)
        {
        }

        protected override void SetupUI()
        {
            base.SetupUI();
            this.Text = "Просмотр товаров (Гость) - ООО Обувь";
        }
    }

    // Форма для клиента
    public class ClientProductsForm : ProductsForm
    {
        public ClientProductsForm(User user) : base(user)
        {
        }

        protected override void SetupUI()
        {
            base.SetupUI();
            this.Text = "Просмотр товаров (Клиент) - ООО Обувь";
        }
    }

    // Форма для менеджера (с возможностью фильтрации/поиска)
    public class ManagerProductsForm : ProductsForm
    {
        private TextBox searchBox;
        private ComboBox categoryFilter;
        private Button searchButton;

        public ManagerProductsForm(User user) : base(user)
        {
        }

        protected override void SetupUI()
        {
            base.SetupUI();
            this.Text = "Управление товарами (Менеджер) - ООО Обувь";

            // Панель фильтров
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(127, 255, 0) // #7FFF00
            };

            // Поле поиска
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Location = new Point(20, 20),
                Size = new Size(50, 25),
                Font = new Font("Times New Roman", 10)
            };

            searchBox = new TextBox
            {
                Location = new Point(80, 20),
                Size = new Size(200, 25),
                Font = new Font("Times New Roman", 10)
            };

            searchButton = new Button
            {
                Text = "Найти",
                Location = new Point(290, 20),
                Size = new Size(80, 25),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.FromArgb(0, 250, 154) // #00FA9A
            };

            searchButton.Click += SearchButton_Click;

            // Фильтр по категории
            var lblCategory = new Label
            {
                Text = "Категория:",
                Location = new Point(400, 20),
                Size = new Size(70, 25),
                Font = new Font("Times New Roman", 10)
            };

            categoryFilter = new ComboBox
            {
                Location = new Point(480, 20),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Кнопка сброса
            var resetButton = new Button
            {
                Text = "Сбросить",
                Location = new Point(650, 20),
                Size = new Size(80, 25),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightGray
            };

            resetButton.Click += ResetButton_Click;

            filterPanel.Controls.AddRange(new Control[] {
                lblSearch, searchBox, searchButton,
                lblCategory, categoryFilter, resetButton
            });

            // Вставляем панель фильтров перед productsFlowPanel
            mainPanel.Controls.Remove(productsFlowPanel);
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(productsFlowPanel);
            productsFlowPanel.Location = new Point(0, 60);
            productsFlowPanel.Height -= 60;

            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT CategoryName FROM Categories ORDER BY CategoryName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        categoryFilter.Items.Add("Все категории");

                        while (reader.Read())
                        {
                            categoryFilter.Items.Add(reader["CategoryName"].ToString());
                        }

                        categoryFilter.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            searchBox.Text = "";
            categoryFilter.SelectedIndex = 0;
            LoadProducts(); // Загружаем все товары заново
        }

        private void ApplyFilters()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT p.*, 
                               s1.SupplierName, 
                               s2.SupplierName as ManufacturerName,
                               c.CategoryName
                        FROM Products p
                        LEFT JOIN Suppliers s1 ON p.SupplierID = s1.SupplierID
                        LEFT JOIN Suppliers s2 ON p.ManufacturerID = s2.SupplierID
                        LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(searchBox.Text))
                    {
                        query += " AND (p.ProductName LIKE @Search OR p.Description LIKE @Search)";
                    }

                    if (categoryFilter.SelectedIndex > 0)
                    {
                        query += " AND c.CategoryName = @Category";
                    }

                    query += " ORDER BY p.ProductName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(searchBox.Text))
                        {
                            command.Parameters.AddWithValue("@Search", $"%{searchBox.Text}%");
                        }

                        if (categoryFilter.SelectedIndex > 0)
                        {
                            command.Parameters.AddWithValue("@Category", categoryFilter.SelectedItem.ToString());
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            var products = new List<Product>();

                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    ProductID = (int)reader["ProductID"],
                                    ArticleNumber = reader["ArticleNumber"].ToString(),
                                    ProductName = reader["ProductName"].ToString(),
                                    UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                    Price = (decimal)reader["Price"],
                                    SupplierID = (int)reader["SupplierID"],
                                    ManufacturerID = (int)reader["ManufacturerID"],
                                    CategoryID = (int)reader["CategoryID"],
                                    CurrentDiscount = (decimal)reader["CurrentDiscount"],
                                    StockQuantity = (int)reader["StockQuantity"],
                                    Description = reader["Description"].ToString(),
                                    PhotoPath = reader["PhotoPath"] != DBNull.Value ? reader["PhotoPath"].ToString() : null,
                                    SupplierName = reader["SupplierName"].ToString(),
                                    ManufacturerName = reader["ManufacturerName"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                };

                                products.Add(product);
                            }

                            DisplayProducts(products);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Форма для администратора (с кнопками редактирования)
    public class AdminProductsForm : ManagerProductsForm
    {
        private Button addButton;
        private Button editButton;
        private Button deleteButton;

        public AdminProductsForm(User user) : base(user)
        {
        }

        protected override void SetupUI()
        {
            base.SetupUI();
            this.Text = "Управление товарами (Администратор) - ООО Обувь";

            // Добавляем кнопки управления в панель фильтров
            var filterPanel = mainPanel.Controls[0] as Panel;

            addButton = new Button
            {
                Text = "Добавить",
                Location = new Point(750, 20),
                Size = new Size(80, 25),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.FromArgb(0, 250, 154) // #00FA9A
            };

            editButton = new Button
            {
                Text = "Редактировать",
                Location = new Point(840, 20),
                Size = new Size(100, 25),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightBlue
            };

            deleteButton = new Button
            {
                Text = "Удалить",
                Location = new Point(950, 20),
                Size = new Size(80, 25),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightCoral
            };

            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;

            filterPanel.Controls.Add(addButton);
            filterPanel.Controls.Add(editButton);
            filterPanel.Controls.Add(deleteButton);

            // Расширяем панель фильтров
            filterPanel.Width = 1050;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var editForm = new AdminProductsEditForm(null);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts(); // Обновляем список
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            // Логика редактирования выбранного товара
            MessageBox.Show("Редактирование товара", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadProductImageIntoControl(ProductItemControl control, Product product)
        {
            // Этот метод будет вызываться из ProductsForm для каждой карточки товара
            // Можно оставить логику загрузки в самом контроле, как выше
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            // Логика удаления товара
            var result = MessageBox.Show("Удалить выбранный товар?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Товар удален", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // Форма редактирования товара для администратора
    public class AdminProductsEditForm : Form
    {
        private Product product;

        public AdminProductsEditForm(Product productToEdit)
        {
            product = productToEdit;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = product == null ? "Добавление товара" : "Редактирование товара";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Times New Roman", 10);

            // Реализация формы редактирования
            // ... (пропускаем для краткости)

            var saveButton = new Button
            {
                Text = "Сохранить",
                Location = new Point(250, 400),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 12),
                BackColor = Color.FromArgb(0, 250, 154) // #00FA9A
            };

            saveButton.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(saveButton);
        }
    }
}