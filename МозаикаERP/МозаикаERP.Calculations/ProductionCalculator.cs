using System;

namespace МозаикаERP.Calculations
{
    public static class ProductionCalculator
    {
        public static int CalculateProductQuantity(
            int productTypeId,
            int materialTypeId,
            int rawMaterialAmount,
            double param1,
            double param2)
        {
            if (rawMaterialAmount <= 0 || param1 <= 0 || param2 <= 0)
                return -1;

            try
            {
                double productCoefficient = GetProductCoefficient(productTypeId);
                double lossPercent = GetLossPercent(materialTypeId);

                if (productCoefficient < 0 || lossPercent < 0)
                    return -1;

                double rawPerUnit = param1 * param2 * productCoefficient;
                double rawPerUnitWithLoss = rawPerUnit * (1 + lossPercent / 100.0);
                int quantity = (int)Math.Floor(rawMaterialAmount / rawPerUnitWithLoss);

                return quantity;
            }
            catch
            {
                return -1;
            }
        }

        private static double GetProductCoefficient(int productTypeId)
        {
            switch (productTypeId)
            {
                case 1: return 1.2;
                case 2: return 1.5;
                case 3: return 1.8;
                case 4: return 2.0;
                default: return -1;
            }
        }

        private static double GetLossPercent(int materialTypeId)
        {
            switch (materialTypeId)
            {
                case 1: return 5.0;
                case 2: return 3.0;
                case 3: return 2.0;
                case 4: return 7.0;
                case 5: return 4.0;
                default: return -1;
            }
        }
    }
}