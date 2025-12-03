using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using МозаикаERP.Data;

namespace МозаикаERP.Services
{
    /// <summary>
    /// Сервис для работы с базой данных SQL Server
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>
        /// Строка подключения к БД из конфигурации
        /// </summary>
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
            ?? @"Data Source=.\SQLEXPRESS;Initial Catalog=Мозаика;Integrated Security=True;MultipleActiveResultSets=True";

        /// <summary>
        /// Проверка доступности базы данных
        /// </summary>
        public static bool IsDatabaseAvailable()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получение списка всех поставщиков
        /// </summary>
        public static DataTable GetSuppliers()
        {
            string query = @"
                SELECT 
                    п.id_поставщика AS 'ID',
                    п.наименование_компании AS 'Название компании',
                    п.тип_поставщика AS 'Тип поставщика',
                    п.инн AS 'ИНН',
                    FORMAT(п.дата_регистрации, 'dd.MM.yyyy') AS 'Дата регистрации',
                    COUNT(DISTINCT м.id_материала) AS 'Кол-во материалов',
                    ISNULL(о.средний_рейтинг, 0.0) AS 'Рейтинг',
                    FORMAT(п.дата_регистрации, 'yyyy-MM-dd') AS 'Начало работы'
                FROM Поставщики п
                LEFT JOIN Материалы м ON п.id_поставщика = м.id_поставщика
                LEFT JOIN (
                    SELECT id_поставщика, AVG(общая_оценка) AS средний_рейтинг
                    FROM ОценкаПоставщиков
                    GROUP BY id_поставщика
                ) о ON п.id_поставщика = о.id_поставщика
                GROUP BY п.id_поставщика, п.наименование_компании, п.тип_поставщика, 
                         п.инн, п.дата_регистрации, о.средний_рейтинг
                ORDER BY о.средний_рейтинг DESC, п.дата_регистрации DESC";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение поставщиков для конкретного материала (Модуль 4)
        /// </summary>
        public static DataTable GetSuppliersForMaterial(int materialId)
        {
            string query = @"
                SELECT 
                    п.id_поставщика AS 'ID',
                    п.наименование_компании AS 'Поставщик',
                    п.тип_поставщика AS 'Тип поставщика',
                    ISNULL(о.средний_рейтинг, 0.0) AS 'Рейтинг',
                    FORMAT(п.дата_регистрации, 'dd.MM.yyyy') AS 'Начало работы',
                    м.наименование_материала AS 'Материал',
                    м.тип_материала AS 'Тип материала',
                    м.цена_за_единицу AS 'Цена за единицу',
                    м.единица_измерения AS 'Единица измерения'
                FROM Поставщики п
                INNER JOIN Материалы м ON п.id_поставщика = м.id_поставщика
                LEFT JOIN (
                    SELECT id_поставщика, AVG(общая_оценка) AS средний_рейтинг
                    FROM ОценкаПоставщиков
                    GROUP BY id_поставщика
                ) о ON п.id_поставщика = о.id_поставщика
                WHERE м.id_материала = @materialId
                ORDER BY о.средний_рейтинг DESC, п.дата_регистрации";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@materialId", materialId);
                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();

                connection.Open();
                adapter.Fill(dataTable);

                return dataTable;
            }
        }

        /// <summary>
        /// Получение заявок с фильтром по статусу
        /// </summary>
        public static DataTable GetOrders(string statusFilter = "Все")
        {
            string query = @"
        SELECT 
            z.id_заявки AS 'ID',
            p.наименование_компании AS 'Партнер',
            FORMAT(z.дата_создания, 'dd.MM.yyyy') AS 'Дата создания',
            z.общая_сумма AS 'Сумма',
            z.статус AS 'Статус',
            s.фио AS 'Менеджер'
        FROM Заявки z
        JOIN Партнеры p ON z.id_партнера = p.id_партнера
        JOIN Сотрудники s ON z.id_менеджера = s.id_сотрудника
        WHERE (@status = 'Все' OR z.статус = @status)
        ORDER BY z.дата_создания DESC";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@status", statusFilter == "Все" ? "НОВАЯ" : statusFilter);

                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();

                connection.Open();
                adapter.Fill(dataTable);

                return dataTable;
            }
        }

        /// <summary>
        /// Получение списка всех материалов
        /// </summary>
        public static DataTable GetMaterials()
        {
            string query = @"
                SELECT 
                    м.id_материала AS 'ID',
                    м.наименование_материала AS 'Наименование',
                    м.тип_материала AS 'Тип',
                    п.наименование_компании AS 'Поставщик',
                    м.количество_в_упаковке AS 'В упаковке',
                    м.единица_измерения AS 'Единица',
                    м.цена_за_единицу AS 'Цена',
                    м.текущий_остаток AS 'Остаток',
                    м.минимальный_запас AS 'Мин. запас',
                    CASE WHEN м.активен = 1 THEN 'Да' ELSE 'Нет' END AS 'Активен'
                FROM Материалы м
                JOIN Поставщики п ON м.id_поставщика = п.id_поставщика
                ORDER BY м.наименование_материала";

            return ExecuteQuery(query);
        }

        // ==================== СОТРУДНИКИ ====================

        /// <summary>
        /// Получение списка всех сотрудников
        /// </summary>
        public static DataTable GetEmployees()
        {
            string query = @"
        SELECT 
            id_сотрудника AS 'ID',
            фио AS 'ФИО',
            FORMAT(дата_рождения, 'dd.MM.yyyy') AS 'Дата рождения',
            паспортные_данные AS 'Паспорт',
            банковские_реквизиты AS 'Банк. реквизиты',
            семейное_положение AS 'Сем. положение',
            состояние_здоровья AS 'Здоровье',
            FORMAT(дата_приема, 'dd.MM.yyyy') AS 'Дата приема'
        FROM Сотрудники
        ORDER BY фио";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение допусков сотрудников
        /// </summary>
        public static DataTable GetEmployeePermits()
        {
            string query = @"
        SELECT 
            д.id_допуска AS 'ID допуска',
            с.фио AS 'Сотрудник',
            д.тип_оборудования AS 'Тип оборудования',
            FORMAT(д.дата_получения_допуска, 'dd.MM.yyyy') AS 'Дата получения',
            FORMAT(д.дата_окончания_допуска, 'dd.MM.yyyy') AS 'Дата окончания',
            д.выдавшая_организация AS 'Выдавшая организация',
            CASE 
                WHEN д.дата_окончания_допуска < GETDATE() THEN '❌ Просрочен'
                WHEN д.дата_окончания_допуска < DATEADD(MONTH, 1, GETDATE()) THEN '⚠️ Скоро истекает'
                ELSE '✅ Активен'
            END AS 'Статус'
        FROM Допуски д
        JOIN Сотрудники с ON д.id_сотрудника = с.id_сотрудника
        ORDER BY д.дата_окончания_допуска";

            return ExecuteQuery(query);
        }

        // ==================== ПРОИЗВОДСТВО ====================

        /// <summary>
        /// Получение производственных заданий
        /// </summary>
        public static DataTable GetProductionTasks()
        {
            string query = @"
        SELECT 
            пз.id_задания AS 'ID задания',
            з.id_заявки AS 'ID заявки',
            с.фио AS 'Мастер',
            п.наименование_компании AS 'Партнер',
            пр.наименование_продукции AS 'Продукция',
            FORMAT(пз.дата_создания, 'dd.MM.yyyy') AS 'Дата создания',
            FORMAT(пз.срок_выполнения, 'dd.MM.yyyy') AS 'Срок выполнения',
            пз.статус AS 'Статус',
            пз.приоритет AS 'Приоритет'
        FROM ПроизводственныеЗадания пз
        JOIN Заявки з ON пз.id_заявки = з.id_заявки
        JOIN Сотрудники с ON пз.id_мастера = с.id_сотрудника
        JOIN Партнеры п ON з.id_партнера = п.id_партнера
        LEFT JOIN ПозицииЗаявки поз ON з.id_заявки = поз.id_заявки
        LEFT JOIN Продукция пр ON поз.id_продукции = пр.id_продукции
        ORDER BY пз.приоритет DESC, пз.срок_выполнения";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение упрощенных производственных данных (если JOIN не работает)
        /// </summary>
        public static DataTable GetProductionSimple()
        {
            string query = @"
        SELECT 
            id_задания AS 'ID задания',
            id_заявки AS 'ID заявки',
            FORMAT(дата_создания, 'dd.MM.yyyy') AS 'Дата создания',
            FORMAT(срок_выполнения, 'dd.MM.yyyy') AS 'Срок выполнения',
            статус AS 'Статус',
            приоритет AS 'Приоритет'
        FROM ПроизводственныеЗадания
        ORDER BY приоритет DESC, срок_выполнения";

            return ExecuteQuery(query);
        }

        // ==================== СКЛАД ====================

        /// <summary>
        /// Получение остатков материалов на складе
        /// </summary>
        public static DataTable GetWarehouseStock()
        {
            string query = @"
        SELECT 
            м.id_материала AS 'ID материала',
            м.наименование_материала AS 'Материал',
            м.тип_материала AS 'Тип материала',
            п.наименование_компании AS 'Поставщик',
            м.текущий_остаток AS 'Текущий остаток',
            м.минимальный_запас AS 'Мин. запас',
            м.единица_измерения AS 'Единица',
            CASE 
                WHEN м.текущий_остаток < м.минимальный_запас THEN '⚠️ Низкий запас'
                WHEN м.текущий_остаток < м.минимальный_запас * 1.5 THEN '⚠️ Внимание'
                ELSE '✅ Норма'
            END AS 'Статус',
            CASE 
                WHEN м.текущий_остаток < м.минимальный_запас THEN 'Срочный заказ'
                WHEN м.текущий_остаток < м.минимальный_запас * 1.5 THEN 'Плановый заказ'
                ELSE 'Запас достаточен'
            END AS 'Рекомендация'
        FROM Материалы м
        JOIN Поставщики п ON м.id_поставщика = п.id_поставщика
        WHERE м.активен = 1
        ORDER BY 
            CASE 
                WHEN м.текущий_остаток < м.минимальный_запас THEN 1
                WHEN м.текущий_остаток < м.минимальный_запас * 1.5 THEN 2
                ELSE 3
            END,
            м.наименование_материала";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение движений материалов
        /// </summary>
        public static DataTable GetWarehouseMovements()
        {
            string query = @"
        SELECT 
            дм.id_движения AS 'ID движения',
            м.наименование_материала AS 'Материал',
            м.тип_материала AS 'Тип материала',
            дм.тип_движения AS 'Тип движения',
            дм.количество AS 'Количество',
            м.единица_измерения AS 'Единица',
            FORMAT(дм.дата_движения, 'dd.MM.yyyy HH:mm') AS 'Дата движения',
            с.фио AS 'Сотрудник',
            з.id_заявки AS 'ID заявки',
            дм.комментарий AS 'Комментарий'
        FROM ДвиженияМатериалов дм
        JOIN Материалы м ON дм.id_материала = м.id_материала
        JOIN Сотрудники с ON дм.id_сотрудника = с.id_сотрудника
        LEFT JOIN Заявки з ON дм.id_связанной_заявки = з.id_заявки
        ORDER BY дм.дата_движения DESC";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение производственных заданий (упрощенная версия)
        /// </summary>
        public static DataTable GetProductionTasksSimple()
        {
            string query = @"
        SELECT 
            id_задания AS 'ID задания',
            id_заявки AS 'ID заявки',
            id_мастера AS 'ID мастера',
            FORMAT(дата_создания, 'dd.MM.yyyy HH:mm') AS 'Дата создания',
            FORMAT(срок_выполнения, 'dd.MM.yyyy') AS 'Срок выполнения',
            статус AS 'Статус',
            приоритет AS 'Приоритет'
        FROM ПроизводственныеЗадания
        ORDER BY приоритет DESC, срок_выполнения";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение производственных заданий с именем мастера
        /// </summary>
        public static DataTable GetProductionTasksWithMaster()
        {
            string query = @"
        SELECT 
            пз.id_задания AS 'ID задания',
            пз.id_заявки AS 'ID заявки',
            с.фио AS 'Мастер',
            FORMAT(пз.дата_создания, 'dd.MM.yyyy HH:mm') AS 'Дата создания',
            FORMAT(пз.срок_выполнения, 'dd.MM.yyyy') AS 'Срок выполнения',
            пз.статус AS 'Статус',
            пз.приоритет AS 'Приоритет'
        FROM ПроизводственныеЗадания пз
        LEFT JOIN Сотрудники с ON пз.id_мастера = с.id_сотрудника
        ORDER BY пз.приоритет DESC, пз.срок_выполнения";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение упрощенных движений материалов (если JOIN не работает)
        /// </summary>
        public static DataTable GetWarehouseMovementsSimple()
        {
            string query = @"
        SELECT 
            id_движения AS 'ID движения',
            FORMAT(дата_движения, 'dd.MM.yyyy HH:mm') AS 'Дата движения',
            тип_движения AS 'Тип движения',
            количество AS 'Количество',
            комментарий AS 'Комментарий'
        FROM ДвиженияМатериалов
        ORDER BY дата_движения DESC";

            return ExecuteQuery(query);
        }

        public static bool AddPartner(string company, string type, string phone, string email, decimal rating)
        {
            try
            {
                string query = @"
            INSERT INTO Партнеры 
            (наименование_компании, тип_партнера, телефон, email, рейтинг, дата_регистрации)
            VALUES (@company, @type, @phone, @email, @rating, GETDATE())";

                var parameters = new[]
                {
            new SqlParameter("@company", company),
            new SqlParameter("@type", type),
            new SqlParameter("@phone", phone),
            new SqlParameter("@email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email),
            new SqlParameter("@rating", rating)
        };

                ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdatePartner(int id, string company, string type, string phone, string email, decimal rating)
        {
            try
            {
                string query = @"
            UPDATE Партнеры 
            SET наименование_компании = @company,
                тип_партнера = @type,
                телефон = @phone,
                email = @email,
                рейтинг = @rating
            WHERE id_партнера = @id";

                var parameters = new[]
                {
            new SqlParameter("@id", id),
            new SqlParameter("@company", company),
            new SqlParameter("@type", type),
            new SqlParameter("@phone", phone),
            new SqlParameter("@email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email),
            new SqlParameter("@rating", rating)
        };

                ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeletePartner(int id)
        {
            try
            {
                string query = "DELETE FROM Партнеры WHERE id_партнера = @id";
                var parameter = new SqlParameter("@id", id);

                ExecuteNonQuery(query, parameter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получение списка партнеров
        /// </summary>
        public static DataTable GetPartners()
        {
            string query = @"
                SELECT 
                    id_партнера AS 'ID',
                    наименование_компании AS 'Компания',
                    тип_партнера AS 'Тип',
                    рейтинг AS 'Рейтинг',
                    телефон AS 'Телефон',
                    email AS 'Email',
                    FORMAT(дата_регистрации, 'dd.MM.yyyy') AS 'Дата регистрации'
                FROM Партнеры
                ORDER BY рейтинг DESC, наименование_компании";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение списка продукции
        /// </summary>
        public static DataTable GetProducts()
        {
            string query = @"
                SELECT 
                    id_продукции AS 'ID',
                    артикул AS 'Артикул',
                    наименование_продукции AS 'Наименование',
                    тип_продукции AS 'Тип',
                    минимальная_цена_партнера AS 'Цена',
                    себестоимость AS 'Себестоимость',
                    номер_цеха AS 'Цех',
                    CASE WHEN активна = 1 THEN 'Да' ELSE 'Нет' END AS 'Активна'
                FROM Продукция
                WHERE активна = 1
                ORDER BY наименование_продукции";

            return ExecuteQuery(query);
        }

        /// <summary>
        /// Получение коэффициентов типов продукции (для Модуля 4)
        /// </summary>
        public static Dictionary<int, double> GetProductTypeCoefficients()
        {
            var coefficients = new Dictionary<int, double>();

            // В реальной БД должна быть таблица с коэффициентами
            // Если нет - используем стандартные значения
            string query = @"
                SELECT DISTINCT тип_продукции, 1.5 AS коэффициент 
                FROM Продукция 
                UNION ALL 
                SELECT 'Керамическая плитка', 1.2 
                UNION ALL 
                SELECT 'Мозаичная плитка', 1.5 
                UNION ALL 
                SELECT 'Керамогранит', 1.8 
                UNION ALL 
                SELECT 'Декоративные элементы', 2.0";

            var dt = ExecuteQuery(query);

            // Преобразуем в словарь (в реальном проекте нужно сопоставить ID с коэффициентами)
            foreach (DataRow row in dt.Rows)
            {
                // Здесь нужно преобразовать тип продукции в ID
                // Пока используем статические значения
            }

            // Статические коэффициенты на случай отсутствия таблицы
            coefficients[1] = 1.2;  // Плитка керамическая
            coefficients[2] = 1.5;  // Плитка мозаичная
            coefficients[3] = 1.8;  // Керамогранит
            coefficients[4] = 2.0;  // Декоративные элементы

            return coefficients;
        }

        /// <summary>
        /// Получение процентов потерь материалов (для Модуля 4)
        /// </summary>
        public static Dictionary<int, double> GetMaterialLossPercentages()
        {
            var percentages = new Dictionary<int, double>();

            // В реальной БД должна быть таблица с процентами потерь
            string query = @"
                SELECT DISTINCT тип_материала, 5.0 AS процент_потерь 
                FROM Материалы 
                UNION ALL 
                SELECT 'Глина', 5.0 
                UNION ALL 
                SELECT 'Песок', 3.0 
                UNION ALL 
                SELECT 'Глазурь', 2.0 
                UNION ALL 
                SELECT 'Красители', 7.0 
                UNION ALL 
                SELECT 'Вспомогательные', 4.0";

            var dt = ExecuteQuery(query);

            // Преобразуем в словарь (в реальном проекте нужно сопоставить ID с процентами)
            foreach (DataRow row in dt.Rows)
            {
                // Здесь нужно преобразовать тип материала в ID
                // Пока используем статические значения
            }

            // Статические проценты на случай отсутствия таблицы
            percentages[1] = 5.0;  // Глина
            percentages[2] = 3.0;  // Песок
            percentages[3] = 2.0;  // Глазурь
            percentages[4] = 7.0;  // Красители
            percentages[5] = 4.0;  // Вспомогательные материалы

            return percentages;
        }

        /// <summary>
        /// Получение коэффициента типа продукции по ID
        /// </summary>
        public static double GetProductCoefficient(int productTypeId)
        {
            try
            {
                // В реальном проекте запрос к таблице коэффициентов
                // string query = "SELECT коэффициент FROM КоэффициентыТиповПродукции WHERE id_типа = @id";

                // Временные статические значения
                var coefficients = new Dictionary<int, double>
                {
                    { 1, 1.2 }, { 2, 1.5 }, { 3, 1.8 }, { 4, 2.0 }
                };

                return coefficients.ContainsKey(productTypeId) ? coefficients[productTypeId] : -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Получение процента потерь материала по ID
        /// </summary>
        public static double GetMaterialLossPercentage(int materialTypeId)
        {
            try
            {
                // В реальном проекте запрос к таблице процентов потерь
                // string query = "SELECT процент_потерь FROM ПроцентыПотерьМатериалов WHERE id_типа = @id";

                // Временные статические значения
                var percentages = new Dictionary<int, double>
                {
                    { 1, 5.0 }, { 2, 3.0 }, { 3, 2.0 }, { 4, 7.0 }, { 5, 4.0 }
                };

                return percentages.ContainsKey(materialTypeId) ? percentages[materialTypeId] : -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Выполнение произвольного SQL запроса с параметрами
        /// </summary>
        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    var adapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();

                    connection.Open();
                    adapter.Fill(dataTable);

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Выполнение SQL команды (INSERT, UPDATE, DELETE)
        /// </summary>
        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения команды: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Выполнение скалярного запроса
        /// </summary>
        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения скалярного запроса: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Добавление нового поставщика
        /// </summary>
        public static int AddSupplier(string companyName, string supplierType, string inn)
        {
            string query = @"
                INSERT INTO Поставщики (наименование_компании, тип_поставщика, инн, дата_регистрации)
                VALUES (@companyName, @supplierType, @inn, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                new SqlParameter("@companyName", companyName),
                new SqlParameter("@supplierType", supplierType),
                new SqlParameter("@inn", inn)
            };

            var result = ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Добавление нового материала
        /// </summary>
        public static int AddMaterial(string materialName, string materialType, int supplierId,
                                      decimal quantityPerPackage, string unit, decimal price,
                                      decimal minStock)
        {
            string query = @"
                INSERT INTO Материалы (наименование_материала, тип_материала, id_поставщика, 
                                      количество_в_упаковке, единица_измерения, цена_за_единицу, 
                                      текущий_остаток, минимальный_запас, активен)
                VALUES (@materialName, @materialType, @supplierId, @quantityPerPackage, 
                       @unit, @price, 0, @minStock, 1);
                SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                new SqlParameter("@materialName", materialName),
                new SqlParameter("@materialType", materialType),
                new SqlParameter("@supplierId", supplierId),
                new SqlParameter("@quantityPerPackage", quantityPerPackage),
                new SqlParameter("@unit", unit),
                new SqlParameter("@price", price),
                new SqlParameter("@minStock", minStock)
            };

            var result = ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Тестирование подключения к БД
        /// </summary>
        public static string TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Проверяем наличие таблиц
                    string checkQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = 'Мозаика'";
                    using (var command = new SqlCommand(checkQuery, connection))
                    {
                        int tableCount = Convert.ToInt32(command.ExecuteScalar());
                        return $"✓ Подключение успешно. Найдено таблиц: {tableCount}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"✗ Ошибка подключения: {ex.Message}";
            }
        }

        /// <summary>
        /// Получение партнеров с возможностью редактирования
        /// </summary>
        public static DataTable GetPartnersEditable()
        {
            string query = @"
        SELECT 
            id_партнера AS 'ID',
            наименование_компании AS 'Компания',
            тип_партнера AS 'Тип',
            рейтинг AS 'Рейтинг',
            телефон AS 'Телефон',
            email AS 'Email',
            дата_регистрации AS 'Дата регистрации'
        FROM Партнеры
        ORDER BY рейтинг DESC, наименование_компании";

            var dt = ExecuteQuery(query);

            // Устанавливаем ключевое поле для обновлений
            dt.PrimaryKey = new DataColumn[] { dt.Columns["ID"] };

            return dt;
        }

        /// <summary>
        /// Получение информации о БД
        /// </summary>
        public static DataTable GetDatabaseInfo()
        {
            string query = @"
                SELECT 
                    TABLE_NAME AS 'Таблица',
                    COUNT(*) AS 'Количество записей'
                FROM INFORMATION_SCHEMA.TABLES t
                LEFT JOIN (
                    SELECT 'Поставщики' AS table_name, COUNT(*) as cnt FROM Поставщики
                    UNION ALL SELECT 'Материалы', COUNT(*) FROM Материалы
                    UNION ALL SELECT 'Сотрудники', COUNT(*) FROM Сотрудники
                    UNION ALL SELECT 'Партнеры', COUNT(*) FROM Партнеры
                    UNION ALL SELECT 'Продукция', COUNT(*) FROM Продукция
                ) c ON t.TABLE_NAME = c.table_name
                WHERE t.TABLE_CATALOG = 'Мозаика' 
                    AND t.TABLE_TYPE = 'BASE TABLE'
                    AND t.TABLE_NAME NOT LIKE '%sys%'
                GROUP BY TABLE_NAME
                ORDER BY TABLE_NAME";

            return ExecuteQuery(query);
        }
    }
}