using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace МозаикаERP.Data.Entities
{
    [Table("Продукция")]
    public class Продукция
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_продукции")]
        public int IdПродукции { get; set; }

        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)]
        public string Артикул { get; set; }

        [Required]
        [StringLength(50)]
        [Column("тип_продукции")]
        public string ТипПродукции { get; set; }

        [Required]
        [StringLength(255)]
        [Column("наименование_продукции")]
        public string НаименованиеПродукции { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Описание { get; set; }

        [Column("минимальная_цена_партнера")]
        public decimal МинимальнаяЦенаПартнера { get; set; }

        [StringLength(100)]
        [Column("размер_упаковки")]
        public string РазмерУпаковки { get; set; }

        [Column("вес_без_упаковки")]
        public decimal? ВесБезУпаковки { get; set; }

        [Column("вес_с_упаковкой")]
        public decimal? ВесСУпаковкой { get; set; }

        [StringLength(100)]
        [Column("номер_стандарта")]
        public string НомерСтандарта { get; set; }

        [Column("время_изготовления_часы")]
        public int ВремяИзготовленияЧасы { get; set; }

        [Column("себестоимость")]
        public decimal Себестоимость { get; set; }

        [Column("номер_цеха")]
        public int НомерЦеха { get; set; }

        [Column("количество_рабочих")]
        public int КоличествоРабочих { get; set; }

        [Column("в_каталоге")]
        public bool ВКаталоге { get; set; } = true;

        [Column("активна")]
        public bool Активна { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<СоставПродукции> СоставПродукции { get; set; }
        public virtual ICollection<ПозицииЗаявки> ПозицииЗаявки { get; set; }
        public virtual ICollection<ИсторияПродаж> ИсторияПродаж { get; set; }
        public virtual ICollection<ИсторияЦенПродукции> ИсторияЦенПродукции { get; set; }
    }
}