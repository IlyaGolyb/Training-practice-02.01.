USE ShoeStoreLLC;
GO

-- 1. Таблица ролей (для нормализации пользователей)
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 2. Таблица поставщиков/производителей (выделена из Tovar)
CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(100) NOT NULL,
    IsManufacturer BIT NOT NULL DEFAULT 1 -- 1 - производитель, 0 - только поставщик
);
GO

-- 3. Таблица категорий товаров
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 4. Таблица товаров (нормализованная)
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleNumber NVARCHAR(20) NOT NULL UNIQUE,
    ProductName NVARCHAR(200) NOT NULL,
    UnitOfMeasure NVARCHAR(10) NOT NULL DEFAULT 'шт.',
    Price DECIMAL(10,2) NOT NULL,
    SupplierID INT NOT NULL,
    ManufacturerID INT NOT NULL,
    CategoryID INT NOT NULL,
    CurrentDiscount DECIMAL(5,2) DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    Description NVARCHAR(MAX),
    PhotoPath NVARCHAR(100),
    CONSTRAINT FK_Products_Supplier FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Products_Manufacturer FOREIGN KEY (ManufacturerID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT CHK_Price CHECK (Price >= 0),
    CONSTRAINT CHK_Discount CHECK (CurrentDiscount >= 0 AND CurrentDiscount <= 100)
);
GO

-- 5. Таблица пунктов выдачи
CREATE TABLE PickupPoints (
    PointID INT IDENTITY(1,1) PRIMARY KEY,
    Address NVARCHAR(200) NOT NULL UNIQUE
);
GO

-- 6. Таблица пользователей (нормализованная)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Login NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

-- 7. Таблица заказов (нормализованная)
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber INT NOT NULL UNIQUE,
    OrderDate DATE NOT NULL,
    DeliveryDate DATE NOT NULL,
    PickupPointID INT NOT NULL,
    UserID INT NOT NULL, -- Клиент, сделавший заказ
    PickupCode NVARCHAR(10) NOT NULL,
    OrderStatus NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Orders_PickupPoints FOREIGN KEY (PickupPointID) REFERENCES PickupPoints(PointID),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT CHK_Dates CHECK (DeliveryDate >= OrderDate)
);
GO

-- 8. Таблица позиций заказа (для связи многие-ко-многим)
CREATE TABLE OrderItems (
    OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    CONSTRAINT CHK_Quantity CHECK (Quantity > 0)
);
GO

-- Заполнение таблиц данными

-- 1. Заполняем роли
INSERT INTO Roles (RoleName) VALUES
('Администратор'),
('Менеджер'),
('Авторизированный клиент');
GO

-- 2. Заполняем поставщиков/производителей
INSERT INTO Suppliers (SupplierName, IsManufacturer) VALUES
('Kari', 1),
('Обувь для вас', 0),
('Marco Tozzi', 1),
('Рос', 1),
('Rieker', 1),
('Alessio Nesca', 1),
('CROSBY', 1),
('Caprice', 1),
('ROMER', 1),
('ARGO', 1),
('Luiza Belly', 1),
('TOFA', 1);
GO

-- 3. Заполняем категории
INSERT INTO Categories (CategoryName) VALUES
('Женская обувь'),
('Мужская обувь');
GO

-- 4. Заполняем товары
INSERT INTO Products (ArticleNumber, ProductName, UnitOfMeasure, Price, SupplierID, ManufacturerID, CategoryID, CurrentDiscount, StockQuantity, Description, PhotoPath) VALUES
('А112Т4', 'Ботинки', 'шт.', 4990.00, 1, 1, 1, 3, 6, 'Женские Ботинки демисезонные kari', '1.jpg'),
('F635R4', 'Ботинки', 'шт.', 3244.00, 2, 3, 1, 2, 13, 'Ботинки Marco Tozzi женские демисезонные, размер 39, цвет бежевый', '2.jpg'),
('H782T5', 'Туфли', 'шт.', 4499.00, 1, 1, 2, 4, 5, 'Туфли kari мужские классика MYZ21AW-450A, размер 43, цвет: черный', '3.jpg'),
('G783F5', 'Ботинки', 'шт.', 5900.00, 1, 4, 2, 2, 8, 'Мужские ботинки Рос-Обувь кожаные с натуральным мехом', '4.jpg'),
('J384T6', 'Ботинки', 'шт.', 3800.00, 2, 5, 2, 2, 16, 'B3430/14 Полуботинки мужские Rieker', '5.jpg'),
('D572U8', 'Кроссовки', 'шт.', 4100.00, 2, 4, 2, 3, 6, '129615-4 Кроссовки мужские', '6.jpg'),
('F572H7', 'Туфли', 'шт.', 2700.00, 1, 3, 1, 2, 14, 'Туфли Marco Tozzi женские летние, размер 39, цвет черный', '7.jpg'),
('D329H3', 'Полуботинки', 'шт.', 1890.00, 2, 6, 1, 4, 4, 'Полуботинки Alessio Nesca женские 3-30797-47, размер 37, цвет: бордовый', '8.jpg'),
('B320R5', 'Туфли', 'шт.', 4300.00, 1, 5, 1, 2, 6, 'Туфли Rieker женские демисезонные, размер 41, цвет коричневый', '9.jpg'),
('G432E4', 'Туфли', 'шт.', 2800.00, 1, 1, 1, 3, 15, 'Туфли kari женские TR-YR-413017, размер 37, цвет: черный', '10.jpg'),
('S213E3', 'Полуботинки', 'шт.', 2156.00, 2, 7, 2, 3, 6, '407700/01-01 Полуботинки мужские CROSBY', NULL),
('E482R4', 'Полуботинки', 'шт.', 1800.00, 1, 1, 1, 2, 14, 'Полуботинки kari женские MYZ20S-149, размер 41, цвет: черный', NULL),
('S634B5', 'Кеды', 'шт.', 5500.00, 2, 8, 2, 3, 0, 'Кеды Caprice мужские демисезонные, размер 42, цвет черный', NULL),
('K345R4', 'Полуботинки', 'шт.', 2100.00, 2, 7, 2, 2, 3, '407700/01-02 Полуботинки мужские CROSBY', NULL),
('O754F4', 'Туфли', 'шт.', 5400.00, 2, 5, 1, 4, 18, 'Туфли женские демисезонные Rieker артикул 55073-68/37', NULL),
('G531F4', 'Ботинки', 'шт.', 6600.00, 1, 1, 1, 12, 9, 'Ботинки женские зимние ROMER арт. 893167-01 Черный', NULL),
('J542F5', 'Тапочки', 'шт.', 500.00, 1, 1, 2, 13, 0, 'Тапочки мужские Арт.70701-55-67син р.41', NULL),
('B431R5', 'Ботинки', 'шт.', 2700.00, 2, 5, 2, 2, 5, 'Мужские кожаные ботинки/мужские ботинки', NULL),
('P764G4', 'Туфли', 'шт.', 6800.00, 1, 7, 1, 15, 15, 'Туфли женские, ARGO, размер 38', NULL),
('C436G5', 'Ботинки', 'шт.', 10200.00, 1, 6, 1, 15, 9, 'Ботинки женские, ARGO, размер 40', NULL),
('F427R5', 'Ботинки', 'шт.', 11800.00, 2, 5, 1, 15, 11, 'Ботинки на молнии с декоративной пряжкой FRAU', NULL),
('N457T5', 'Полуботинки', 'шт.', 4600.00, 1, 7, 1, 3, 13, 'Полуботинки Ботинки черные зимние, мех', NULL),
('D364R4', 'Туфли', 'шт.', 12400.00, 1, 1, 1, 16, 5, 'Туфли Luiza Belly женские Kate-lazo черные из натуральной замши', NULL),
('S326R5', 'Тапочки', 'шт.', 9900.00, 2, 7, 2, 17, 15, 'Мужские кожаные тапочки "Профиль С.Дали"', NULL),
('L754R4', 'Полуботинки', 'шт.', 1700.00, 1, 1, 1, 2, 7, 'Полуботинки kari женские WB2020SS-26, размер 38, цвет: черный', NULL),
('M542T5', 'Кроссовки', 'шт.', 2800.00, 2, 5, 2, 18, 3, 'Кроссовки мужские TOFA', NULL),
('D268G5', 'Туфли', 'шт.', 4399.00, 2, 5, 1, 3, 12, 'Туфли Rieker женские демисезонные, размер 36, цвет коричневый', NULL),
('T324F5', 'Сапоги', 'шт.', 4699.00, 1, 7, 1, 2, 5, 'Сапоги замша Цвет: синий', NULL),
('K358H6', 'Тапочки', 'шт.', 599.00, 1, 5, 2, 20, 2, 'Тапочки мужские син р.41', NULL),
('H535R5', 'Ботинки', 'шт.', 2300.00, 2, 5, 1, 2, 7, 'Женские Ботинки демисезонные', NULL);
GO

-- 5. Заполняем пункты выдачи (первые 20 из списка)
INSERT INTO PickupPoints (Address) VALUES
('420151, г. Лесной, ул. Вишневая, 32'),
('125061, г. Лесной, ул. Подгорная, 8'),
('630370, г. Лесной, ул. Шоссейная, 24'),
('400562, г. Лесной, ул. Зеленая, 32'),
('614510, г. Лесной, ул. Маяковского, 47'),
('410542, г. Лесной, ул. Светлая, 46'),
('620839, г. Лесной, ул. Цветочная, 8'),
('443890, г. Лесной, ул. Коммунистическая, 1'),
('603379, г. Лесной, ул. Спортивная, 46'),
('603721, г. Лесной, ул. Гоголя, 41'),
('410172, г. Лесной, ул. Северная, 13'),
('614611, г. Лесной, ул. Молодежная, 50'),
('454311, г.Лесной, ул. Новая, 19'),
('660007, г.Лесной, ул. Октябрьская, 19'),
('603036, г. Лесной, ул. Садовая, 4'),
('394060, г.Лесной, ул. Фрунзе, 43'),
('410661, г. Лесной, ул. Школьная, 50'),
('625590, г. Лесной, ул. Коммунистическая, 20'),
('625683, г. Лесной, ул. 8 Марта'),
('450983, г.Лесной, ул. Комсомольская, 26');
GO

-- 6. Заполняем пользователей
INSERT INTO Users (FullName, Login, PasswordHash, RoleID) VALUES
('Никифорова Весения Николаевна', '94d5ous@gmail.com', 'uzWC67', 1),
('Сазонов Руслан Германович', 'uth4iz@mail.com', '2L6KZG', 1),
('Одинцов Серафим Артёмович', 'yzls62@outlook.com', 'JlFRCZ', 1),
('Степанов Михаил Артёмович', '1diph5e@tutanota.com', '8ntwUp', 2),
('Ворсин Петр Евгеньевич', 'tjde7c@yahoo.com', 'YOyhfR', 2),
('Старикова Елена Павловна', 'wpmrc3do@tutanota.com', 'RSbvHv', 2),
('Михайлюк Анна Вячеславовна', '5d4zbu@tutanota.com', 'rwVDh9', 3),
('Ситдикова Елена Анатольевна', 'ptec8ym@yahoo.com', 'LdNyos', 3),
('Ворсин Петр Евгеньевич', '1qz4kw@mail.com', 'gynQMT', 3),
('Старикова Елена Павловна', '4np6se@mail.com', 'AtnDjr', 3);
GO

-- 7. Заполняем заказы
INSERT INTO Orders (OrderNumber, OrderDate, DeliveryDate, PickupPointID, UserID, PickupCode, OrderStatus) VALUES
(1, '2025-02-27', '2025-04-20', 1, 4, '901', 'Завершен'),
(2, '2022-09-28', '2025-04-21', 11, 1, '902', 'Завершен'),
(3, '2025-03-21', '2025-04-22', 2, 2, '903', 'Завершен'),
(4, '2025-02-20', '2025-04-23', 11, 3, '904', 'Завершен'),
(5, '2025-03-17', '2025-04-24', 2, 4, '905', 'Завершен'),
(6, '2025-03-01', '2025-04-25', 15, 1, '906', 'Завершен'),
(7, '2025-02-28', '2025-04-26', 3, 2, '907', 'Завершен'),
(8, '2025-03-31', '2025-04-27', 19, 3, '908', 'Новый'),
(9, '2025-04-02', '2025-04-28', 5, 4, '909', 'Новый'),
(10, '2025-04-03', '2025-04-29', 19, 4, '910', 'Новый');
GO

-- 8. Заполняем позиции заказов
INSERT INTO OrderItems (OrderID, ProductID, Quantity) VALUES
(1, 1, 2),  -- А112Т4, 2
(1, 2, 2),  -- F635R4, 2
(2, 3, 1),  -- H782T5, 1
(2, 4, 1),  -- G783F5, 1
(3, 5, 10), -- J384T6, 10
(3, 6, 10), -- D572U8, 10
(4, 7, 5),  -- F572H7, 5
(4, 8, 4),  -- D329H3, 4
(5, 1, 2),  -- А112Т4, 2
(5, 2, 2),  -- F635R4, 2
(6, 3, 1),  -- H782T5, 1
(6, 4, 1),  -- G783F5, 1
(7, 5, 10), -- J384T6, 10
(7, 6, 10), -- D572U8, 10
(8, 7, 5),  -- F572H7, 5
(8, 8, 4),  -- D329H3, 4
(9, 9, 5),  -- B320R5, 5
(9, 10, 1), -- G432E4, 1
(10, 11, 5), -- S213E3, 5
(10, 12, 5); -- E482R4, 5
GO

-- Создание индексов для оптимизации
CREATE INDEX IX_Products_ArticleNumber ON Products(ArticleNumber);
CREATE INDEX IX_Products_Category ON Products(CategoryID);
CREATE INDEX IX_Users_Login ON Users(Login);
CREATE INDEX IX_Orders_Status ON Orders(OrderStatus);
CREATE INDEX IX_Orders_User ON Orders(UserID);
GO