using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using МозаикаERP.Data.Entities;

namespace МозаикаERP.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=DefaultConnection")
        {
            // Настройки для повышения производительности
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Убираем множественное число в названиях таблиц
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Указываем схему (если нужно)
            modelBuilder.HasDefaultSchema("dbo");

            base.OnModelCreating(modelBuilder);
        }

        // Все таблицы из БД
        public DbSet<Поставщики> Поставщики { get; set; }
        public DbSet<Материалы> Материалы { get; set; }
        public DbSet<Сотрудники> Сотрудники { get; set; }
        public DbSet<Допуски> Допуски { get; set; }
        public DbSet<Партнеры> Партнеры { get; set; }
        public DbSet<Продукция> Продукция { get; set; }
        public DbSet<СоставПродукции> СоставПродукции { get; set; }
        public DbSet<Заявки> Заявки { get; set; }
        public DbSet<ПозицииЗаявки> ПозицииЗаявки { get; set; }
        public DbSet<ИсторияПродаж> ИсторияПродаж { get; set; }
        public DbSet<ИсторияРейтинга> ИсторияРейтинга { get; set; }
        public DbSet<ДвиженияМатериалов> ДвиженияМатериалов { get; set; }
        public DbSet<ДоступСотрудников> ДоступСотрудников { get; set; }
        public DbSet<ПроизводственныеЗадания> ПроизводственныеЗадания { get; set; }
        public DbSet<ИсторияЦенПродукции> ИсторияЦенПродукции { get; set; }
        public DbSet<ОценкаПоставщиков> ОценкаПоставщиков { get; set; }
    }
}