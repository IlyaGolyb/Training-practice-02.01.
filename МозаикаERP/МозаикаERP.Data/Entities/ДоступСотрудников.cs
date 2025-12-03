using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("ДоступСотрудников")]
    public class ДоступСотрудников
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_записи")]
        public int IdЗаписи { get; set; }

        [Column("id_сотрудника")]
        [ForeignKey("Сотрудник")]
        public int IdСотрудника { get; set; }

        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)]
        [Column("номер_карты")]
        public string НомерКарты { get; set; }

        [Column("дата_прохода")]
        public DateTime ДатаПрохода { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Column("тип_прохода")]
        public string ТипПрохода { get; set; }

        [StringLength(100)]
        [Column("точка_доступа")]
        public string ТочкаДоступа { get; set; }

        // Навигационные свойства
        public virtual Сотрудники Сотрудник { get; set; }
    }
}