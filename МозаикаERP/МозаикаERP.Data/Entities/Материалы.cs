using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Материалы")]
    public class Материалы
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_материала")]
        public int IdМатериала { get; set; }

        [Required]
        [StringLength(50)]
        [Column("тип_материала")]
        public string ТипМатериала { get; set; }

        [Required]
        [StringLength(255)]
        [Column("наименование_материала")]
        public string НаименованиеМатериала { get; set; }

        [Column("id_поставщика")]
        [ForeignKey("Поставщик")]
        public int IdПоставщика { get; set; }

        [Column("количество_в_упаковке")]
        public decimal КоличествоВУпаковке { get; set; }

        [Required]
        [StringLength(20)]
        [Column("единица_измерения")]
        public string ЕдиницаИзмерения { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Описание { get; set; }

        [Column("цена_за_единицу")]
        public decimal ЦенаЗаЕдиницу { get; set; }

        [Column("текущий_остаток")]
        public decimal ТекущийОстаток { get; set; } = 0;

        [Column("минимальный_запас")]
        public decimal МинимальныйЗапас { get; set; }

        [Column("активен")]
        public bool Активен { get; set; } = true;

        // Навигационные свойства
        public virtual Поставщики Поставщик { get; set; }
        public virtual ICollection<СоставПродукции> СоставПродукции { get; set; }
        public virtual ICollection<ДвиженияМатериалов> ДвиженияМатериалов { get; set; }
    }
}