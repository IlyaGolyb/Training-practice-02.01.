using ShoeStoreLLC.Models;
using ShopApp.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShopApp.Controls
{
    public class ProductItemControl : UserControl
    {
        private Product product;
        private PictureBox pictureBox;

        public ProductItemControl(Product product)
        {
            this.product = product;
            InitializeControl();
        }

        private void InitializeControl()
        {
            this.Size = new Size(900, 200);
            this.BackColor = GetBackgroundColor();
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(10);

            // === ЛЕВАЯ ЧАСТЬ - ФОТО ===
            var photoPanel = new Panel
            {
                Size = new Size(180, 180),
                Location = new Point(10, 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            pictureBox = new PictureBox
            {
                Size = new Size(170, 170),
                Location = new Point(5, 5),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            LoadProductImage();

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
            this.Controls.Add(photoPanel);
            this.Controls.Add(infoPanel);
        }

        private void LoadProductImage()
        {
            // Пробуем загрузить фото из поля PhotoPath (например: "1.jpg")
            if (!string.IsNullOrEmpty(product.PhotoPath))
            {
                // Сначала пробуем путь из поля (если указан полный путь)
                if (System.IO.File.Exists(product.PhotoPath))
                {
                    LoadImageFromFile(product.PhotoPath);
                    return;
                }

                // Пробуем найти файл в папке Resources
                string resourcesPath = System.IO.Path.Combine("Resources", product.PhotoPath);
                if (System.IO.File.Exists(resourcesPath))
                {
                    LoadImageFromFile(resourcesPath);
                    return;
                }

                // Пробуем в папке с исполняемым файлом
                string appPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.PhotoPath);
                if (System.IO.File.Exists(appPath))
                {
                    LoadImageFromFile(appPath);
                    return;
                }

                // Пробуем в папке с исполняемым файлом + Resources
                string appResourcesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", product.PhotoPath);
                if (System.IO.File.Exists(appResourcesPath))
                {
                    LoadImageFromFile(appResourcesPath);
                    return;
                }
            }

            // Если фото не найдено, используем заглушку picture.png
            LoadDefaultImage();
        }

        private void LoadImageFromFile(string filePath)
        {
            try
            {
                pictureBox.Image = Image.FromFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения {filePath}: {ex.Message}");
                LoadDefaultImage();
            }
        }

        private void LoadDefaultImage()
        {
            try
            {
                // Пробуем загрузить picture.png из Resources
                string defaultImagePath = "picture.png";

                // Ищем в разных местах
                string[] possiblePaths = {
                    System.IO.Path.Combine("Resources", defaultImagePath),
                    defaultImagePath,
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", defaultImagePath),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultImagePath)
                };

                foreach (string path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        pictureBox.Image = Image.FromFile(path);
                        return;
                    }
                }

                // Если picture.png не найден, создаем программную заглушку
                CreateFallbackImage();
            }
            catch
            {
                CreateFallbackImage();
            }
        }

        private void CreateFallbackImage()
        {
            // Создаем программную заглушку
            var bmp = new Bitmap(170, 170);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawRectangle(Pens.Black, 0, 0, 169, 169);

                // Рисуем иконку обуви (используем символ эмодзи или текст)
                using (var font = new Font("Times New Roman", 14, FontStyle.Bold))
                {
                    // Пытаемся нарисовать эмодзи или текст
                    try
                    {
                        g.DrawString("👞", font, Brushes.Black, 60, 60);
                    }
                    catch
                    {
                        g.DrawString("Обувь", font, Brushes.Black, 50, 60);
                    }
                }

                g.DrawString("Нет фото", new Font("Times New Roman", 10), Brushes.Black, 55, 120);
            }
            pictureBox.Image = bmp;
        }

        private Color GetBackgroundColor()
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

        // Метод для обновления товара
        public void UpdateProduct(Product newProduct)
        {
            product = newProduct;
            this.BackColor = GetBackgroundColor();
            LoadProductImage();
        }
    }
}