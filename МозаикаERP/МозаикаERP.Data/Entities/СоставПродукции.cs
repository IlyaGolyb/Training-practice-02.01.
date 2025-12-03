using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("СоставПродукции")]
    public class СоставПродукции
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_состава")]
        public int IdСостава { get; set; }

        [Column("id_продукции")]
        [ForeignKey("Продукция")]
        public int IdПродукции { get; set; }

        [Column("id_материала")]
        [ForeignKey("Материал")]
        public int IdМатериала { get; set; }

        [Column("количество_материала")]
        public decimal КоличествоМатериала { get; set; }

        [Required]
        [StringLength(20)]
        [Column("единица_измерения")]
        public string ЕдиницаИзмерения { get; set; }

        // Навигационные свойства
        public virtual Продукция Продукция { get; set; }
        public virtual Материалы Материал { get; set; }
    }
}