-- Создаем папку для backup если не существует
EXEC xp_create_subdir 'E:\4 курс\УП 02.01\Отчеты\задание 3\Backup';
GO

-- Резервное копирование
BACKUP DATABASE Мозайка 
TO DISK = 'E:\4 курс\УП 02.01\Отчеты\задание 3\Backup\MosaicCompany_Backup.bak'
WITH 
    FORMAT,
    NAME = 'Full Backup of MosaicCompany',
    DESCRIPTION = 'Полная резервная копия базы данных Мозайка',
    COMPRESSION,
    STATS = 10;
GO

PRINT 'Резервная копия создана успешно';