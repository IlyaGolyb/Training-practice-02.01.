USE Мозайка;
GO

-- Простая тестовая загрузка
BULK INSERT Поставщики
FROM 'E:\4 курс\УП 02.01\Отчеты\задание 3\Данные\Поставщики.csv'
WITH (
    FIELDTERMINATOR = ';',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2
);
GO

SELECT * FROM Поставщики;