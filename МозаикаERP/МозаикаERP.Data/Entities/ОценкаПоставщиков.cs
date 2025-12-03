using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ОценкаПоставщиков")]
    public class ОценкаПоставщиков
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_оценки")]
        public int IdОценки { get; set; }

        [Column("id_поставщика")]
        [ForeignKey("Поставщик")]
        public int IdПоставщика { get; set; }

        [Column("id_аналитика")]
        [ForeignKey("Аналитик")]
        public int IdАналитика { get; set; }

        [Column("дата_оценки")]
        public DateTime ДатаОценки { get; set; } = DateTime.Now;

        [Column("качество_материалов")]
        public int? КачествоМатериалов { get; set; }

        [Column("надежность_поставок")]
        public int? НадежностьПоставок { get; set; }

        [Column("соответствие_срокам")]
        public int? СоответствиеСрокам { get; set; }

        [Column("общая_оценка")]
        public decimal? ОбщаяОценка { get; set; }

        [StringLength(500)]
        public string Рекомендация { get; set; }

        // Навигационные свойства
        public virtual Поставщики Поставщик { get; set; }
        public virtual Сотрудники Аналитик { get; set; }
    }
}