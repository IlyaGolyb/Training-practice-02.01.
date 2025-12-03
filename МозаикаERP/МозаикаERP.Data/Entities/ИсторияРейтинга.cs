using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ИсторияРейтинга")]
    public class ИсторияРейтинга
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_записи")]
        public int IdЗаписи { get; set; }

        [Column("id_партнера")]
        [ForeignKey("Партнер")]
        public int IdПартнера { get; set; }

        [Column("старый_рейтинг")]
        public decimal? СтарыйРейтинг { get; set; }

        [Column("новый_рейтинг")]
        public decimal НовыйРейтинг { get; set; }

        [Column("дата_изменения")]
        public DateTime ДатаИзменения { get; set; } = DateTime.Now;

        [Column("изменил_сотрудник")]
        [ForeignKey("ИзменившийСотрудник")]
        public int ИзменилСотрудник { get; set; }

        [StringLength(500)]
        public string Причина { get; set; }

        // Навигационные свойства
        public virtual Партнеры Партнер { get; set; }
        public virtual Сотрудники ИзменившийСотрудник { get; set; }
    }
}