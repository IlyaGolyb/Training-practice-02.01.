using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ИсторияЦенПродукции")]
    public class ИсторияЦенПродукции
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_записи")]
        public int IdЗаписи { get; set; }

        [Column("id_продукции")]
        [ForeignKey("Продукция")]
        public int IdПродукции { get; set; }

        [Column("старая_цена")]
        public decimal? СтараяЦена { get; set; }

        [Column("новая_цена")]
        public decimal НоваяЦена { get; set; }

        [Column("дата_изменения")]
        public DateTime ДатаИзменения { get; set; } = DateTime.Now;

        [Column("изменил_сотрудник")]
        [ForeignKey("ИзменившийСотрудник")]
        public int ИзменилСотрудник { get; set; }

        [StringLength(500)]
        public string Причина { get; set; }

        // Навигационные свойства
        public virtual Продукция Продукция { get; set; }
        public virtual Сотрудники ИзменившийСотрудник { get; set; }
    }
}