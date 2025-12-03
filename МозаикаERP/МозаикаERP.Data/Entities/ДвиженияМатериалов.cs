using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ДвиженияМатериалов")]
    public class ДвиженияМатериалов
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_движения")]
        public int IdДвижения { get; set; }

        [Column("id_материала")]
        [ForeignKey("Материал")]
        public int IdМатериала { get; set; }

        [Required]
        [StringLength(50)]
        [Column("тип_движения")]
        public string ТипДвижения { get; set; }

        [Column("количество")]
        public decimal Количество { get; set; }

        [Column("дата_движения")]
        public DateTime ДатаДвижения { get; set; } = DateTime.Now;

        [Column("id_связанной_заявки")]
        [ForeignKey("СвязаннаяЗаявка")]
        public int? IdСвязаннойЗаявки { get; set; }

        [Column("id_сотрудника")]
        [ForeignKey("Сотрудник")]
        public int IdСотрудника { get; set; }

        [StringLength(500)]
        public string Комментарий { get; set; }

        // Навигационные свойства
        public virtual Материалы Материал { get; set; }
        public virtual Заявки СвязаннаяЗаявка { get; set; }
        public virtual Сотрудники Сотрудник { get; set; }
    }
}