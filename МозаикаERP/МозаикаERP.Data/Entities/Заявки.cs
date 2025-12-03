using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Заявки")]
    public class Заявки
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_заявки")]
        public int IdЗаявки { get; set; }

        [Column("id_партнера")]
        [ForeignKey("Партнер")]
        public int IdПартнера { get; set; }

        [Column("id_менеджера")]
        [ForeignKey("Менеджер")]
        public int IdМенеджера { get; set; }

        [Column("дата_создания")]
        public DateTime ДатаСоздания { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Статус { get; set; } = "НОВАЯ";

        [Column("сумма_предоплаты")]
        public decimal СуммаПредоплаты { get; set; } = 0;

        [Column("общая_сумма")]
        public decimal ОбщаяСумма { get; set; }

        [Column("размер_скидки")]
        public decimal РазмерСкидки { get; set; } = 0;

        [Column("дата_выполнения")]
        public DateTime? ДатаВыполнения { get; set; }

        [Column("дата_предоплаты")]
        public DateTime? ДатаПредоплаты { get; set; }

        [Column("дата_полной_оплаты")]
        public DateTime? ДатаПолнойОплаты { get; set; }

        [StringLength(100)]
        [Column("способ_доставки")]
        public string СпособДоставки { get; set; }

        [Column("дата_отгрузки")]
        public DateTime? ДатаОтгрузки { get; set; }

        [StringLength(500)]
        [Column("результат_проверки_качества")]
        public string РезультатПроверкиКачества { get; set; }

        [Column("дата_автоотмены")]
        public DateTime? ДатаАвтоотмены { get; set; }

        [Column("уведомление_отправлено")]
        public bool УведомлениеОтправлено { get; set; } = false;

        // Навигационные свойства
        public virtual Партнеры Партнер { get; set; }
        public virtual Сотрудники Менеджер { get; set; }
        public virtual ICollection<ПозицииЗаявки> ПозицииЗаявки { get; set; }
        public virtual ICollection<ДвиженияМатериалов> ДвиженияМатериалов { get; set; }
        public virtual ICollection<ПроизводственныеЗадания> ПроизводственныеЗадания { get; set; }
    }
}