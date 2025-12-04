-- Запуск процедуры
EXEC dbo.usp_BackupShoeStoreLLC;
GO

DECLARE @fileName NVARCHAR(400);
DECLARE @dateTime NVARCHAR(20);

-- Форматируем дату и время для имени файла
SET @dateTime = REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '-');
SET @dateTime = REPLACE(@dateTime, ' ', '_');

-- Указываем путь на вашем компьютере
SET @fileName = 'E:\4 курс\УП 02.01\Пробный демоэкзамен\Проект\Backup\ShoeStoreLLC_' + @dateTime + '.bak';

-- Выполняем резервное копирование
BACKUP DATABASE ShoeStoreLLC 
TO DISK = @fileName
WITH FORMAT,
     NAME = 'ShoeStoreLLC Backup ' + @dateTime,
     COMPRESSION,
     STATS = 10;

PRINT 'Резервная копия создана: ' + @fileName;
GO