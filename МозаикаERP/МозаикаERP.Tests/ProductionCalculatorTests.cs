using Microsoft.VisualStudio.TestTools.UnitTesting;
using МозаикаERP.Calculations;
using System;

namespace МозаикаERP.Tests
{
    [TestClass]
    public class ProductionCalculatorTests
    {
        [TestMethod]
        [TestCategory("Positive")]
        [Description("Позитивный тест: Успешный расчет продукции")]
        public void CalculateProductQuantity_ValidData_ReturnsPositiveQuantity()
        {
            // Arrange
            int productTypeId = 1;
            int materialTypeId = 1;
            int rawMaterialAmount = 1000;
            double param1 = 2.5;
            double param2 = 3.0;

            // Act
            int result = ProductionCalculator.CalculateProductQuantity(
                productTypeId, materialTypeId, rawMaterialAmount, param1, param2);

            // Assert
            Assert.IsTrue(result > 0);
            Console.WriteLine($"Результат расчета: {result} единиц продукции");
        }

        [TestMethod]
        [TestCategory("Negative")]
        [Description("Негативный тест: Расчет с несуществующим типом продукции")]
        public void CalculateProductQuantity_InvalidProductType_ReturnsMinusOne()
        {
            // Arrange
            int invalidProductTypeId = 999;

            // Act
            int result = ProductionCalculator.CalculateProductQuantity(
                invalidProductTypeId, 1, 1000, 2.5, 3.0);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [Description("Негативный тест: Расчет с нулевым количеством сырья")]
        public void CalculateProductQuantity_ZeroRawMaterial_ReturnsMinusOne()
        {
            // Arrange
            int zeroRawMaterial = 0;

            // Act
            int result = ProductionCalculator.CalculateProductQuantity(
                1, 1, zeroRawMaterial, 2.5, 3.0);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [TestCategory("Positive")]
        [Description("Тест разных типов продукции и материалов")]
        public void CalculateProductQuantity_DifferentTypes_ReturnsDifferentResults()
        {
            // Arrange & Act
            int result1 = ProductionCalculator.CalculateProductQuantity(1, 1, 1000, 2.5, 3.0);
            int result2 = ProductionCalculator.CalculateProductQuantity(2, 2, 1000, 2.5, 3.0);
            int result3 = ProductionCalculator.CalculateProductQuantity(3, 3, 1000, 2.5, 3.0);

            // Assert
            Assert.AreNotEqual(result1, result2,
                "Разные типы продукции должны давать разный результат");
            Assert.AreNotEqual(result2, result3,
                "Разные типы материалов должны влиять на результат");

            Console.WriteLine($"Тип 1: {result1}, Тип 2: {result2}, Тип 3: {result3}");
        }
    }
}