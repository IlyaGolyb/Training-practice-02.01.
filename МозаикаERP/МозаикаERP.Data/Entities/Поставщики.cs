using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Поставщики")]
    public class Поставщики
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_поставщика")]
        public int IdПоставщика { get; set; }

        [Required]
        [StringLength(50)]
        [Column("тип_поставщика")]
        public string ТипПоставщика { get; set; }

        [Required]
        [StringLength(255)]
        [Column("наименование_компании")]
        public string НаименованиеКомпании { get; set; }

        [Required]
        [StringLength(12)]
        [Index(IsUnique = true)]
        public string Инн { get; set; }

        [Column("дата_регистрации")]
        public DateTime ДатаРегистрации { get; set; } = DateTime.Now;

        // Навигационные свойства
        public virtual ICollection<Материалы> Материалы { get; set; }
        public virtual ICollection<ОценкаПоставщиков> Оценки { get; set; }
    }
}