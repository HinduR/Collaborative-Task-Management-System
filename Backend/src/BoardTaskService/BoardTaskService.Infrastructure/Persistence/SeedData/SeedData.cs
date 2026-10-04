
using System.Globalization;
using System.Reflection;
using System.Text;
using BoardTaskService.Domain.Models;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Model;

namespace BoardTaskService.Infrastructure.Persistence.SeedData;

/// <summary>
/// Provides seed data initialization for the IdentityService database.
/// </summary>
public static class SeedData
{
    private static BoardTaskDbContext? _dbContext;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _dbContext = serviceProvider.GetRequiredService<BoardTaskDbContext>();

        Assembly assembly = Assembly.GetExecutingAssembly();

        List<Project> refSets = ReadEmbeddedCsv<Project>(
            assembly,
            "BoardTaskService.Infrastructure.Persistence.SeedData.Project.csv");

        AddEntities(refSets);
        SaveEntities();

      
        SaveEntities();
    }
    private static List<T> ReadEmbeddedCsv<T>(
        Assembly assembly,
        string resourceName)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                string.Concat("Embedded resource not found: ",
                    resourceName,
                    "."));

        return ReadCsv<T>(stream);
    }

    public static List<T> ReadCsv<T>(Stream fileStream)
    {
        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            IgnoreReferences = true
        };

        using StreamReader reader = new(fileStream, Encoding.UTF8);
        using CsvReader csvReader = new(reader, config);
        return csvReader.GetRecords<T>().ToList();
    }

    public static void AddEntities<T>(IEnumerable<T> entries) where T : BaseModel
    {
        foreach (T entry in entries)
        {
            PropertyInfo? idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null) continue;

            object? idValue = idProperty.GetValue(entry);

            T? existingEntry = _dbContext!.Set<T>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefault(e => EF.Property<object>(e, "Id")!.Equals(idValue));

            if (existingEntry == null)
            {
                _dbContext.Add(entry);
            }
            else
            {
                (DateTime CreatedAt, DateTime UpdatedAt, Guid CreatedBy, Guid UpdatedBy, bool IsActive) baseProps =
                    BackupBaseProperties(existingEntry);

                T? local = _dbContext.Set<T>().Local
                    .FirstOrDefault(e => idProperty.GetValue(e)!.Equals(idValue));
                if (local != null)
                    _dbContext.Entry(local).State = EntityState.Detached;

                _dbContext.Entry(entry).State = EntityState.Modified;
                RestoreBaseProperties(entry, baseProps);
            }
        }
    }

    public static void SaveEntities()
    {
        // Using a fixed system/seed user id since there's no logged-in user during seeding
        _dbContext!.SaveAsync(Guid.Empty).GetAwaiter().GetResult();
    }

    private static (DateTime CreatedAt, DateTime UpdatedAt, Guid CreatedBy, Guid UpdatedBy, bool IsActive)
        BackupBaseProperties<T>(T entity) where T : BaseModel
        => (entity.CreatedAt, entity.UpdatedAt, entity.CreatedBy, entity.UpdatedBy, entity.IsActive);

    private static void RestoreBaseProperties<T>(T entity,
        (DateTime CreatedAt, DateTime UpdatedAt, Guid CreatedBy, Guid UpdatedBy, bool IsActive) baseProps)
        where T : BaseModel
    {
        entity.CreatedAt = baseProps.CreatedAt;
        entity.IsActive = baseProps.IsActive;
        entity.UpdatedAt = DateTime.Now;   // matches your project's DateTime.Now convention
        entity.UpdatedBy = baseProps.UpdatedBy;
        entity.CreatedBy = baseProps.CreatedBy;
    }
}
