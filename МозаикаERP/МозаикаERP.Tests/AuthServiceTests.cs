using Microsoft.VisualStudio.TestTools.UnitTesting;
using МозаикаERP.Services;
using System;

namespace МозаикаERP.Tests
{
    [TestClass]
    public class AuthServiceTests
    {
        [TestMethod]
        [TestCategory("Positive")]
        [Description("Позитивный тест: Успешная авторизация администратора")]
        public void Login_ValidAdminCredentials_ReturnsTrue()
        {
            try
            {
                // Arrange
                string username = "admin";
                string password = "admin123";

                // Act
                bool result = AuthService.Login(username, password);

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual("Администратор", AuthService.CurrentRole);

                Console.WriteLine($"Авторизация {username} успешна, роль: {AuthService.CurrentRole}");
            }
            finally
            {
                // Сброс состояния после теста
                AuthService.Logout();
            }
        }

        [TestMethod]
        [TestCategory("Negative")]
        [Description("Негативный тест: Неуспешная авторизация")]
        public void Login_InvalidCredentials_ReturnsFalse()
        {
            // Arrange
            string username = "wronguser";
            string password = "wrongpass";

            // Act
            bool result = AuthService.Login(username, password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("Positive")]
        [Description("Позитивный тест: Доступ к Модулю 4 для Менеджера")]
        public void HasAccessToModule4_ManagerRole_ReturnsTrue()
        {
            try
            {
                // Arrange
                AuthService.Login("manager", "manager123");

                // Act
                bool hasAccess = AuthService.HasAccessToModule4();

                // Assert
                Assert.IsTrue(hasAccess);
                Console.WriteLine($"Менеджер имеет доступ к Модулю 4: {hasAccess}");
            }
            finally
            {
                AuthService.Logout();
            }
        }

        [TestMethod]
        [TestCategory("Negative")]
        [Description("Негативный тест: Доступ к модулю без прав")]
        public void HasAccessToModule_MasterToPartners_ReturnsFalse()
        {
            try
            {
                // Arrange
                AuthService.Login("master", "master123");

                // Act
                bool hasAccess = AuthService.HasAccessToModule("Партнеры");

                // Assert
                Assert.IsFalse(hasAccess);
                Console.WriteLine($"Мастер не имеет доступ к Партнерам: {hasAccess}");
            }
            finally
            {
                AuthService.Logout();
            }
        }
    }
}