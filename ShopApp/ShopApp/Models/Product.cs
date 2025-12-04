using System;

namespace ShoeStoreLLC.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ArticleNumber { get; set; }
        public string ProductName { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Price { get; set; }
        public int SupplierID { get; set; }
        public int ManufacturerID { get; set; }
        public int CategoryID { get; set; }
        public decimal CurrentDiscount { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; }
        public string PhotoPath { get; set; }

        // Навигационные свойства
        public string SupplierName { get; set; }
        public string ManufacturerName { get; set; }
        public string CategoryName { get; set; }

        // Вычисляемые свойства
        public decimal FinalPrice => Math.Round(Price * (1 - CurrentDiscount / 100), 2);
        public bool HasDiscount => CurrentDiscount > 0;
        public bool IsOutOfStock => StockQuantity == 0;
        public bool HasBigDiscount => CurrentDiscount > 15;

        public string DisplayPrice
        {
            get
            {
                if (HasDiscount)
                {
                    return $"{Price:N0} ₽ → {FinalPrice:N0} ₽";
                }
                return $"{Price:N0} ₽";
            }
        }
    }
}