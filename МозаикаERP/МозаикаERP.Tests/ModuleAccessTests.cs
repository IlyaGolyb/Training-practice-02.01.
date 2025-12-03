using Microsoft.VisualStudio.TestTools.UnitTesting;
using МозаикаERP.Services;
using System;

namespace МозаикаERP.Tests
{
    [TestClass]
    public class ModuleAccessTests
    {
        [TestMethod]
        [TestCategory("Positive")]
        [Description("Тест доступа к модулям для Администратора")]
        public void Admin_HasAccessToAllModules()
        {
            try
            {
                // Arrange
                AuthService.Login("admin", "admin123");
                var modules = new[]
                {
                    "Партнеры", "Заявки", "Продукция", "Материалы",
                    "Производство", "Сотрудники", "Отчетность", "Модуль 4"
                };

                // Act & Assert
                foreach (var module in modules)
                {
                    bool hasAccess = AuthService.HasAccessToModule(module);
                    Assert.IsTrue(hasAccess,
                        $"Админ должен иметь доступ к модулю '{module}'");
                }
            }
            finally
            {
                AuthService.Logout();
            }
        }

        [TestMethod]
        [TestCategory("Negative")]
        [Description("Тест отсутствия доступа Аналитика к Производству")]
        public void Analyst_NoAccessToProduction()
        {
            try
            {
                // Arrange
                AuthService.Login("analyst", "analyst123");

                // Act
                bool hasAccess = AuthService.HasAccessToModule("Производство");

                // Assert
                Assert.IsFalse(hasAccess,
                    "Аналитик не должен иметь доступ к Производству");
            }
            finally
            {
                AuthService.Logout();
            }
        }
    }
}