using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgresEfExample;
using samplenpgsql;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Console.Write("Введите режим работы (0 - запись + чтение, 1 - только чтение): ");
if (!int.TryParse(Console.ReadLine(), out int mode) || (mode != 0 && mode != 1))
{
    Console.WriteLine("Некорректный режим работы. Завершение программы.");
    return;
}

await using (var context = new ApplicationDbContext())
{
    try
    {
        // Создание базы данных и таблицы, если они не существуют
        await context.Database.MigrateAsync();
        Console.WriteLine("База данных и таблицы успешно созданы.");

        if (mode == 0)
        {
            await RunWriteAndRead(context);
        }
        else if (mode == 1)
        {
            //await RunReadOnly(context);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Произошла ошибка: {ex.Message}");
    }
}

async Task RunWriteAndRead(ApplicationDbContext context)
{
    var cts = new CancellationTokenSource();
    var cancellationToken = cts.Token;

    // Генерация мок-данных
    var mockContractors = new List<Contractor>();
    for (int i = 0; i < 100; i++)
    {
        var newContractor = new Contractor
        {
            Name = $"Компания {i}",
            Inn = new Random().Next(100000000, 999999999),
            L = new Random().NextInt64(-92233720, 92233720),
            Created = DateTime.MaxValue,
            Deleted = false,
            LongData = "{\"ApplicationId\":\"6eb6e39b-c856-44db-9f9b-1ab853897b5d\",\"Description\":\"Микросервис хранения пользовательских настроек UI\",\"Ports\":[12050],\"Urls\":[],\"Name\":\"UiSettings.Api\",\"CurrentVersion\":\"3.10.6.2\",\"SearchServicePattern\":\"UiSettings.Api_*.*.*.*\",\"SourcePaths\":[\"services\",\"ui_settings_api\"],\"BuildPaths\":null,\"SolutionName\":\"UiSettings.sln\",\"ExecutableName\":\"UiSettings.Api\",\"ProjectGroup\":2,\"LastOnline\":null,\"LastOnlineCheck\":null,\"HealthStatus\":0,\"RegistrationTime\":null,\"IsAutoStart\":false,\"IsAutoRestart\":false,\"AutoStartOrder\":10,\"AutoRestartTimeOut\":0,\"CurrentFolder\":\"C:\\\\Spargo\\\\Repository\\\\UiSettings.Api_3.10.6.2\",\"IsOnline\":false,\"DisplayName\":null,\"DateCreated\":\"2025-02-15T21:52:26.978789+03:00\",\"DateModified\":\"2025-02-15T21:52:50.4476837+03:00\",\"DateDeleted\":null,\"version\":0,\"IdGlobal\":\"6eb6e39b-c856-44db-9f9b-1ab853897b5d\",\"DomainEvents\":null}"
        };
        mockContractors.Add(newContractor);
        //На всякий случай дубль
        mockContractors.Add(newContractor);
    }

    // Начало транзакции
    using var tx = DbTools.TransactionScopeAsyncCreate();
    //await using (var transaction = await context.Database.BeginTransactionAsync(
    //isolationLevel: System.Data.IsolationLevel.ReadCommitted,
    //cancellationToken: cancellationToken))
    {
        // Asynchronous
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
        {
            try
            {

                var task1 = ctx.AddTask("[green]Чтение и запись данных[/]");
                task1.MaxValue(mockContractors.Count);
                for (var idx = 0; idx < mockContractors.Count; idx++)
                {
                    var contractor = mockContractors[idx];
                    context.UpdateOrInsertAsync(contractor, cancellationToken);
                    task1.Increment(1);
                    // Сохранение изменений
                }
                await context.SaveChangesAsync(cancellationToken);
                context.ChangeTracker.Clear();

                //var task2 = ctx.AddTask("[green]Чтение данных[/]");
                //task2.MaxValue(mockContractors.Count);

                //for (var idx = 0; idx < mockContractors.Count; idx++)
                //{
                //    var contractor = mockContractors[idx];
                //    var dbEntity = context.Contractors
                //                .Where(x => x.Id == contractor.Id)
                //                .AsNoTracking()
                //                .OrderBy(x => x.Created)
                //                .Take(1)
                //                .ToList();
                //    task2.Increment(1);
                //}

                // Фиксация транзакции
                //await transaction.CommitAsync(cancellationToken);
                AnsiConsole.WriteLine("Мок-данные успешно добавлены.");
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync(cancellationToken);
                AnsiConsole.WriteLine($"Произошла ошибка при записи данных:");
                AnsiConsole.WriteException(ex);
                return;
            }

        });

    }

    // Чтение данных
    //var contractors = await context.Contractors.ToListAsync(cancellationToken);
   // Console.WriteLine("Данные из таблицы CONTRACTORS:");
  //  foreach (var contractor in contractors.Take(10)) // Вывод первых 10 записей
   // {
  //      Console.WriteLine($"ID: {contractor.Id}, Название: {contractor.Name}, ИНН: {contractor.Inn}, L: {contractor.L}, Created: {contractor.Created}, Deleted: {contractor.Deleted}, LongData: {contractor.LongData}");
   // }
    tx.Complete();
    NpgsqlConnection.ClearAllPools();

    async Task RunReadOnly(ApplicationDbContext context)
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        Console.Write("Введите количество итераций чтения: ");
        if (!int.TryParse(Console.ReadLine(), out int iterations) || iterations <= 0)
        {
            Console.WriteLine("Некорректное количество итераций.");
            return;
        }

        Random random = new();

        for (int i = 0; i < iterations; i++)
        {
            int limit = random.Next(500, 100000); // Случайный лимит от 1 до 100
            Console.WriteLine($"\nИтерация {i + 1}: Чтение лимит {limit} записей...");
            try
            {
                var contractors = await context.Contractors.Take(limit)
                    .Skip(random.Next(1, 100000))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                Console.Write($"{contractors.Count} contractors");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            NpgsqlConnection.ClearAllPools();


            //foreach (var contractor in contractors)
            //{
            //    Console.WriteLine($"ID: {contractor.Id}, Название: {contractor.Name}, ИНН: {contractor.Inn}, L: {contractor.L}, Created: {contractor.Created}, Deleted: {contractor.Deleted}, LongData: {contractor.LongData}");
            //}
        }
    }
}

namespace PostgresEfExample
{
    public class Contractor 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Timestamp]
        public uint Version { get; set; }
        public string Name { get; set; }
        public int Inn { get; set; }
        public long L { get; set; }
        public DateTime? Created { get; set; }
        public bool Deleted { get; set; }
        public string LongData { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Contractor> Contractors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к PostgreSQL
            optionsBuilder.UseNpgsql("User ID=postgres;Password=password;Server=192.168.1.20;Port=5432;Database=test;Pooling=true;Include Error Detail=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contractor>(entity =>
            {
                entity.ToTable("CONTRACTORS"); // Имя таблицы в базе данных

                // Первичный ключ
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .IsRequired();

                // Название контрагента (не может быть NULL)
                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .IsRequired();

                // Создание неуникального индекса по полю Name для ускорения поиска
                entity.HasIndex(e => e.Name)
                    .IsUnique(false) // Разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_NAME");

                // ИНН контрагента (не может быть NULL)
                entity.Property(e => e.Inn)
                    .HasColumnName("INN")
                    .IsRequired();

                // Создание уникального индекса по полю Inn для предотвращения дублей
                entity.HasIndex(e => e.Inn)
                    .IsUnique(false) //разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_INN");

                // Длинное числовое значение
                entity.Property(e => e.L)
                    .HasColumnName("L")
                    .IsRequired();

                // Создание неуникального индекса по полю L для ускорения поиска
                entity.HasIndex(e => e.L)
                    .IsUnique(false) // Разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_L");

                // Дата создания
                entity.Property(e => e.Created)
                    .HasColumnName("CREATED")
                    .HasColumnType("timestamp with time zone");

                // Создание неуникального индекса по полю Created для ускорения сортировки и фильтрации
                entity.HasIndex(e => e.Created)
                    .IsUnique(false) // Разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_CREATED");

                // Флаг удаления (по умолчанию false)
                entity.Property(e => e.Deleted)
                    .HasColumnName("DELETED")
                    .HasDefaultValue(false);

                // Создание неуникального индекса по полю Deleted для оптимизации запросов с фильтрацией по этому полю
                entity.HasIndex(e => e.Deleted)
                    .IsUnique(false) // Разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_DELETED");

                // Большой текст
                entity.Property(e => e.LongData)
                    .HasColumnName("LONG_DATA")
                    .HasMaxLength(4000); // Ограничение длины текста

                // Создание неуникального индекса по первым 256 символам поля LongData для ускорения поиска
                entity.HasIndex(e => new { e.LongData })
                    .IsUnique(false) // Разрешаем дубликаты
                    .HasDatabaseName("IX_CONTRACTORS_LONGDATA")
                    .HasFilter("CHAR_LENGTH(\"LONG_DATA\") > 0"); // Индексируется только непустые значения
            });
        }

        public void Insert(Contractor contractor, CancellationToken cancellationToken = default)
        {
            Contractors.Add(contractor);
        }

        public void  UpdateOrInsertAsync(Contractor contractor, CancellationToken cancellationToken = default)
        {
            var dbEntity = Contractors
                .Where(x => x.Id == contractor.Id)
                .AsNoTracking()
                .OrderBy(x => x.Created)
                .Take(1)
                .ToList();

            if (dbEntity.Any())
            {
                var dbe = dbEntity.First();
                //this.Entry(dbe).State = EntityState.Modified;
               // this.Entry(dbe).CurrentValues.SetValues(contractor);
            }
            else
            {
                Insert(contractor, cancellationToken);
            }
        }
    }
}