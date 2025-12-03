using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ИсторияПродаж")]
    public class ИсторияПродаж
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_записи")]
        public int IdЗаписи { get; set; }

        [Column("id_партнера")]
        [ForeignKey("Партнер")]
        public int IdПартнера { get; set; }

        [Column("id_продукции")]
        [ForeignKey("Продукция")]
        public int IdПродукции { get; set; }

        [Column("дата_продажи")]
        public DateTime ДатаПродажи { get; set; }

        [Column("количество")]
        public int Количество { get; set; }

        [Column("сумма_продажи")]
        public decimal СуммаПродажи { get; set; }

        [Column("примененная_скидка")]
        public decimal ПримененнаяСкидка { get; set; } = 0;

        // Навигационные свойства
        public virtual Партнеры Партнер { get; set; }
        public virtual Продукция Продукция { get; set; }
    }
}