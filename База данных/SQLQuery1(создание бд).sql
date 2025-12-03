-- Создаем базу данных заново
CREATE DATABASE Мозаика;
GO

USE Мозаика;
GO

-- 1. Таблица Поставщики
CREATE TABLE Поставщики (
    id_поставщика INT PRIMARY KEY IDENTITY(1,1),
    тип_поставщика NVARCHAR(50) NOT NULL,
    наименование_компании NVARCHAR(255) NOT NULL,
    инн NVARCHAR(12) NOT NULL UNIQUE,
    дата_регистрации DATETIME DEFAULT GETDATE()
);
GO

-- 2. Таблица Материалы
CREATE TABLE Материалы (
    id_материала INT PRIMARY KEY IDENTITY(1,1),
    тип_материала NVARCHAR(50) NOT NULL,
    наименование_материала NVARCHAR(255) NOT NULL,
    id_поставщика INT NOT NULL,
    количество_в_упаковке DECIMAL(10,2) NOT NULL,
    единица_измерения NVARCHAR(20) NOT NULL,
    описание NVARCHAR(MAX),
    цена_за_единицу DECIMAL(15,2) NOT NULL,
    текущий_остаток DECIMAL(10,2) NOT NULL DEFAULT 0,
    минимальный_запас DECIMAL(10,2) NOT NULL,
    активен BIT DEFAULT 1,
    FOREIGN KEY (id_поставщика) REFERENCES Поставщики(id_поставщика)
);
GO

-- 3. Таблица Сотрудники
CREATE TABLE Сотрудники (
    id_сотрудника INT PRIMARY KEY IDENTITY(1,1),
    фио NVARCHAR(255) NOT NULL,
    дата_рождения DATE NOT NULL,
    паспортные_данные NVARCHAR(100) NOT NULL UNIQUE,
    банковские_реквизиты NVARCHAR(255),
    семейное_положение NVARCHAR(50),
    состояние_здоровья NVARCHAR(MAX),
    дата_приема DATE NOT NULL DEFAULT GETDATE()
);
GO

-- 4. Таблица Допуски
CREATE TABLE Допуски (
    id_допуска INT PRIMARY KEY IDENTITY(1,1),
    id_сотрудника INT NOT NULL,
    тип_оборудования NVARCHAR(100) NOT NULL,
    дата_получения_допуска DATE NOT NULL,
    дата_окончания_допуска DATE NOT NULL,
    выдавшая_организация NVARCHAR(255) NOT NULL,
    FOREIGN KEY (id_сотрудника) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 5. Таблица Партнеры
CREATE TABLE Партнеры (
    id_партнера INT PRIMARY KEY IDENTITY(1,1),
    тип_партнера NVARCHAR(50) NOT NULL,
    наименование_компании NVARCHAR(255) NOT NULL,
    юридический_адрес NVARCHAR(500) NOT NULL,
    инн NVARCHAR(12) NOT NULL UNIQUE,
    фио_директора NVARCHAR(255) NOT NULL,
    телефон NVARCHAR(20) NOT NULL,
    email NVARCHAR(255),
    рейтинг DECIMAL(3,2) DEFAULT 5.0,
    места_продаж NVARCHAR(500),
    дата_регистрации DATETIME DEFAULT GETDATE()
);
GO

-- 6. Таблица Продукция
CREATE TABLE Продукция (
    id_продукции INT PRIMARY KEY IDENTITY(1,1),
    артикул NVARCHAR(50) NOT NULL UNIQUE,
    тип_продукции NVARCHAR(50) NOT NULL,
    наименование_продукции NVARCHAR(255) NOT NULL,
    описание NVARCHAR(MAX),
    минимальная_цена_партнера DECIMAL(15,2) NOT NULL,
    размер_упаковки NVARCHAR(100),
    вес_без_упаковки DECIMAL(8,2),
    вес_с_упаковкой DECIMAL(8,2),
    номер_стандарта NVARCHAR(100),
    время_изготовления_часы INT NOT NULL,
    себестоимость DECIMAL(15,2) NOT NULL,
    номер_цеха INT NOT NULL,
    количество_рабочих INT NOT NULL,
    в_каталоге BIT DEFAULT 1,
    активна BIT DEFAULT 1
);
GO

-- 7. Таблица СоставПродукции
CREATE TABLE СоставПродукции (
    id_состава INT PRIMARY KEY IDENTITY(1,1),
    id_продукции INT NOT NULL,
    id_материала INT NOT NULL,
    количество_материала DECIMAL(10,2) NOT NULL,
    единица_измерения NVARCHAR(20) NOT NULL,
    FOREIGN KEY (id_продукции) REFERENCES Продукция(id_продукции),
    FOREIGN KEY (id_материала) REFERENCES Материалы(id_материала)
);
GO

-- 8. Таблица Заявки
CREATE TABLE Заявки (
    id_заявки INT PRIMARY KEY IDENTITY(1,1),
    id_партнера INT NOT NULL,
    id_менеджера INT NOT NULL,
    дата_создания DATETIME DEFAULT GETDATE(),
    статус NVARCHAR(50) NOT NULL DEFAULT 'НОВАЯ',
    сумма_предоплаты DECIMAL(15,2) DEFAULT 0,
    общая_сумма DECIMAL(15,2) NOT NULL,
    размер_скидки DECIMAL(5,2) DEFAULT 0,
    дата_выполнения DATE,
    дата_предоплаты DATETIME,
    дата_полной_оплаты DATETIME,
    способ_доставки NVARCHAR(100),
    дата_отгрузки DATETIME,
    результат_проверки_качества NVARCHAR(500),
    дата_автоотмены DATETIME,
    уведомление_отправлено BIT DEFAULT 0,
    FOREIGN KEY (id_партнера) REFERENCES Партнеры(id_партнера),
    FOREIGN KEY (id_менеджера) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 9. Таблица ПозицииЗаявки
CREATE TABLE ПозицииЗаявки (
    id_позиции INT PRIMARY KEY IDENTITY(1,1),
    id_заявки INT NOT NULL,
    id_продукции INT NOT NULL,
    количество INT NOT NULL,
    цена_за_единицу DECIMAL(15,2) NOT NULL,
    дата_производства DATE,
    статус NVARCHAR(50) DEFAULT 'ОЖИДАЕТ',
    FOREIGN KEY (id_заявки) REFERENCES Заявки(id_заявки),
    FOREIGN KEY (id_продукции) REFERENCES Продукция(id_продукции)
);
GO

-- 10. Таблица ИсторияПродаж
CREATE TABLE ИсторияПродаж (
    id_записи INT PRIMARY KEY IDENTITY(1,1),
    id_партнера INT NOT NULL,
    id_продукции INT NOT NULL,
    дата_продажи DATE NOT NULL,
    количество INT NOT NULL,
    сумма_продажи DECIMAL(15,2) NOT NULL,
    примененная_скидка DECIMAL(5,2) DEFAULT 0,
    FOREIGN KEY (id_партнера) REFERENCES Партнеры(id_партнера),
    FOREIGN KEY (id_продукции) REFERENCES Продукция(id_продукции)
);
GO

-- 11. Таблица ИсторияРейтинга
CREATE TABLE ИсторияРейтинга (
    id_записи INT PRIMARY KEY IDENTITY(1,1),
    id_партнера INT NOT NULL,
    старый_рейтинг DECIMAL(3,2),
    новый_рейтинг DECIMAL(3,2) NOT NULL,
    дата_изменения DATETIME DEFAULT GETDATE(),
    изменил_сотрудник INT NOT NULL,
    причина NVARCHAR(500),
    FOREIGN KEY (id_партнера) REFERENCES Партнеры(id_партнера),
    FOREIGN KEY (изменил_сотрудник) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 12. Таблица ДвиженияМатериалов
CREATE TABLE ДвиженияМатериалов (
    id_движения INT PRIMARY KEY IDENTITY(1,1),
    id_материала INT NOT NULL,
    тип_движения NVARCHAR(50) NOT NULL,
    количество DECIMAL(10,2) NOT NULL,
    дата_движения DATETIME DEFAULT GETDATE(),
    id_связанной_заявки INT,
    id_сотрудника INT NOT NULL,
    комментарий NVARCHAR(500),
    FOREIGN KEY (id_материала) REFERENCES Материалы(id_материала),
    FOREIGN KEY (id_связанной_заявки) REFERENCES Заявки(id_заявки),
    FOREIGN KEY (id_сотрудника) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 13. Таблица ДоступСотрудников
CREATE TABLE ДоступСотрудников (
    id_записи INT PRIMARY KEY IDENTITY(1,1),
    id_сотрудника INT NOT NULL,
    номер_карты NVARCHAR(50) NOT NULL UNIQUE,
    дата_прохода DATETIME DEFAULT GETDATE(),
    тип_прохода NVARCHAR(50),
    точка_доступа NVARCHAR(100),
    FOREIGN KEY (id_сотрудника) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 14. Таблица ПроизводственныеЗадания
CREATE TABLE ПроизводственныеЗадания (
    id_задания INT PRIMARY KEY IDENTITY(1,1),
    id_заявки INT NOT NULL,
    id_мастера INT NOT NULL,
    дата_создания DATETIME DEFAULT GETDATE(),
    срок_выполнения DATE,
    статус NVARCHAR(50) DEFAULT 'НАЗНАЧЕНО',
    приоритет INT DEFAULT 1,
    FOREIGN KEY (id_заявки) REFERENCES Заявки(id_заявки),
    FOREIGN KEY (id_мастера) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 15. Таблица ИсторияЦенПродукции
CREATE TABLE ИсторияЦенПродукции (
    id_записи INT PRIMARY KEY IDENTITY(1,1),
    id_продукции INT NOT NULL,
    старая_цена DECIMAL(15,2),
    новая_цена DECIMAL(15,2) NOT NULL,
    дата_изменения DATETIME DEFAULT GETDATE(),
    изменил_сотрудник INT NOT NULL,
    причина NVARCHAR(500),
    FOREIGN KEY (id_продукции) REFERENCES Продукция(id_продукции),
    FOREIGN KEY (изменил_сотрудник) REFERENCES Сотрудники(id_сотрудника)
);
GO

-- 16. Таблица ОценкаПоставщиков
CREATE TABLE ОценкаПоставщиков (
    id_оценки INT PRIMARY KEY IDENTITY(1,1),
    id_поставщика INT NOT NULL,
    id_аналитика INT NOT NULL,
    дата_оценки DATE DEFAULT GETDATE(),
    качество_материалов INT,
    надежность_поставок INT,
    соответствие_срокам INT,
    общая_оценка DECIMAL(3,2),
    рекомендация NVARCHAR(500),
    FOREIGN KEY (id_поставщика) REFERENCES Поставщики(id_поставщика),
    FOREIGN KEY (id_аналитика) REFERENCES Сотрудники(id_сотрудника)
);
GO