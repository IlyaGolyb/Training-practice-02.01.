using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ПозицииЗаявки")]
    public class ПозицииЗаявки
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_позиции")]
        public int IdПозиции { get; set; }

        [Column("id_заявки")]
        [ForeignKey("Заявка")]
        public int IdЗаявки { get; set; }

        [Column("id_продукции")]
        [ForeignKey("Продукция")]
        public int IdПродукции { get; set; }

        [Column("количество")]
        public int Количество { get; set; }

        [Column("цена_за_единицу")]
        public decimal ЦенаЗаЕдиницу { get; set; }

        [Column("дата_производства")]
        public DateTime? ДатаПроизводства { get; set; }

        [StringLength(50)]
        public string Статус { get; set; } = "ОЖИДАЕТ";

        // Навигационные свойства
        public virtual Заявки Заявка { get; set; }
        public virtual Продукция Продукция { get; set; }
    }
}