using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Партнеры")]
    public class Партнеры
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_партнера")]
        public int IdПартнера { get; set; }

        [Required]
        [StringLength(50)]
        [Column("тип_партнера")]
        public string ТипПартнера { get; set; }

        [Required]
        [StringLength(255)]
        [Column("наименование_компании")]
        public string НаименованиеКомпании { get; set; }

        [Required]
        [StringLength(500)]
        [Column("юридический_адрес")]
        public string ЮридическийАдрес { get; set; }

        [Required]
        [StringLength(12)]
        [Index(IsUnique = true)]
        public string Инн { get; set; }

        [Required]
        [StringLength(255)]
        [Column("фио_директора")]
        public string ФиоДиректора { get; set; }

        [Required]
        [StringLength(20)]
        public string Телефон { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Column("рейтинг")]
        public decimal Рейтинг { get; set; } = 5.0m;

        [StringLength(500)]
        [Column("места_продаж")]
        public string МестаПродаж { get; set; }

        [Column("дата_регистрации")]
        public DateTime ДатаРегистрации { get; set; } = DateTime.Now;

        // Навигационные свойства
        public virtual ICollection<Заявки> Заявки { get; set; }
        public virtual ICollection<ИсторияПродаж> ИсторияПродаж { get; set; }
        public virtual ICollection<ИсторияРейтинга> ИсторияРейтинга { get; set; }
    }
}