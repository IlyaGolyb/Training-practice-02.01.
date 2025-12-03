using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Допуски")]
    public class Допуски
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_допуска")]
        public int IdДопуска { get; set; }

        [Column("id_сотрудника")]
        [ForeignKey("Сотрудник")]
        public int IdСотрудника { get; set; }

        [Required]
        [StringLength(100)]
        [Column("тип_оборудования")]
        public string ТипОборудования { get; set; }

        [Column("дата_получения_допуска")]
        public DateTime ДатаПолученияДопуска { get; set; }

        [Column("дата_окончания_допуска")]
        public DateTime ДатаОкончанияДопуска { get; set; }

        [Required]
        [StringLength(255)]
        [Column("выдавшая_организация")]
        public string ВыдавшаяОрганизация { get; set; }

        // Навигационные свойства
        public virtual Сотрудники Сотрудник { get; set; }
    }
}