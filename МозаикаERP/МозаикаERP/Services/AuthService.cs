// Создайте новый файл AuthService.cs
using System.Collections.Generic;

public static class AuthService
{
    // Хранилище пользователей (в реальном проекте - в БД)
    private static Dictionary<string, (string Password, string Role)> users = new Dictionary<string, (string, string)>
    {
        // Формат: Логин -> (Пароль, Роль)
        { "admin", ("admin123", "Администратор") },
        { "manager", ("manager123", "Менеджер") },
        { "analyst", ("analyst123", "Аналитик") },
        { "warehouse", ("warehouse123", "Кладовщик") },
        { "master", ("master123", "Мастер") }
    };

    // Текущий пользователь
    public static string CurrentUser { get; private set; }
    public static string CurrentRole { get; private set; }
    public static bool IsAuthenticated { get; private set; }

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    public static bool Login(string username, string password)
    {
        if (users.ContainsKey(username.ToLower()))
        {
            var user = users[username.ToLower()];
            if (user.Password == password) // В реальном проекте - хэширование!
            {
                CurrentUser = username;
                CurrentRole = user.Role;
                IsAuthenticated = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    public static void Logout()
    {
        CurrentUser = null;
        CurrentRole = null;
        IsAuthenticated = false;
    }

    /// <summary>
    /// Проверка доступа к модулю
    /// </summary>
    public static bool HasAccessToModule(string moduleName)
    {
        if (!IsAuthenticated) return false;
        if (CurrentRole == "Администратор") return true;

        Dictionary<string, string[]> permissions = new Dictionary<string, string[]>
    {
        { "Менеджер", new[] { "Заявки", "Продукция", "Отчетность", "Партнеры" } },
        { "Аналитик", new[] { "Продукция", "Отчетность" } },
        { "Кладовщик", new[] { "Материалы", "Склад", "Отчетность" } },
        { "Мастер", new[] { "Производство", "Материалы" } }
    };

        if (permissions.ContainsKey(CurrentRole))
        {
            string[] allowedModules = permissions[CurrentRole];
            foreach (var module in allowedModules)
            {
                if (module == moduleName)
                    return true;
            }
            return false;
        }

        return false;
    }

    /// <summary>
    /// Проверка доступа к Модулю 4 (особый случай)
    /// </summary>
    public static bool HasAccessToModule4()
    {
        if (!IsAuthenticated) return false;

        // Кому доступен Модуль 4: Администратор, Менеджер, Кладовщик
        string[] allowedRoles = { "Администратор", "Менеджер", "Кладовщик" };

        foreach (var role in allowedRoles)
        {
            if (CurrentRole == role)
                return true;
        }

        return false;
    }
}