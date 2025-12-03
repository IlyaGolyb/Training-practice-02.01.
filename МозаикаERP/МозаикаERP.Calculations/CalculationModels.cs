
namespace МозаикаERP.Calculations
{
    public class ProductionCalculationRequest
    {
        public int ProductTypeId { get; set; }
        public int MaterialTypeId { get; set; }
        public int RawMaterialAmount { get; set; }
        public double Parameter1 { get; set; }
        public double Parameter2 { get; set; }
    }

    public class ProductionCalculationResult
    {
        public bool Success { get; set; }
        public int ProductQuantity { get; set; }
        public string ErrorMessage { get; set; }
    }
}