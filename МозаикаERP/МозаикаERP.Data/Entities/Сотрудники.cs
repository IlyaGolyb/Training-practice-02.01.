using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Сотрудники")]
    public class Сотрудники
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_сотрудника")]
        public int IdСотрудника { get; set; }

        [Required]
        [StringLength(255)]
        public string Фио { get; set; }

        [Column("дата_рождения")]
        public DateTime ДатаРождения { get; set; }

        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        [Column("паспортные_данные")]
        public string ПаспортныеДанные { get; set; }

        [StringLength(255)]
        [Column("банковские_реквизиты")]
        public string БанковскиеРеквизиты { get; set; }

        [StringLength(50)]
        [Column("семейное_положение")]
        public string СемейноеПоложение { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]  // <-- УБИРАЕМ Column здесь, он уже указан ниже
        public string СостояниеЗдоровья { get; set; }  // <-- Column не нужен здесь

        [Column("дата_приема")]
        public DateTime ДатаПриема { get; set; } = DateTime.Now;

        // Навигационные свойства
        public virtual ICollection<Допуски> Допуски { get; set; }
        public virtual ICollection<Заявки> ЗаявкиКакМенеджер { get; set; }
        public virtual ICollection<ПроизводственныеЗадания> ЗаданияКакМастер { get; set; }
        public virtual ICollection<ДвиженияМатериалов> ДвиженияМатериалов { get; set; }
        public virtual ICollection<ДоступСотрудников> ДоступыСотрудников { get; set; }
    }
}