using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ПроизводственныеЗадания")]
    public class ПроизводственныеЗадания
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_задания")]
        public int IdЗадания { get; set; }

        [Column("id_заявки")]
        [ForeignKey("Заявка")]
        public int IdЗаявки { get; set; }

        [Column("id_мастера")]
        [ForeignKey("Мастер")]
        public int IdМастера { get; set; }

        [Column("дата_создания")]
        public DateTime ДатаСоздания { get; set; } = DateTime.Now;

        [Column("срок_выполнения")]
        public DateTime? СрокВыполнения { get; set; }

        [StringLength(50)]
        public string Статус { get; set; } = "НАЗНАЧЕНО";

        [Column("приоритет")]
        public int Приоритет { get; set; } = 1;

        // Навигационные свойства
        public virtual Заявки Заявка { get; set; }
        public virtual Сотрудники Мастер { get; set; }
    }
}