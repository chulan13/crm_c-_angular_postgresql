using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using abcomm_test.Models;

namespace abcomm_test.Data;

public static class CsvDataInitializer
{
    private static readonly string[] DepartmentColumns = ["id", "name"];
    private static readonly string[] StatusColumns = ["id", "name"];
    private static readonly string[] TaskColumns =
    [
        "id", "name", "status_id", "department_id", "assignee",
        "deadline", "description", "cancel_reason"
    ];

    public static async Task InitializeAsync(
        CrmDbContext db,
        string contentRootPath,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.CrmTasks.AnyAsync(cancellationToken)
            || await db.Departments.AnyAsync(cancellationToken)
            || await db.Statuses.AnyAsync(cancellationToken))
        {
            logger.LogInformation("CRM database already contains data; CSV import skipped.");
            return;
        }

        var dataPath = Path.Combine(contentRootPath, "testData");
        var departments = ReadDepartments(Path.Combine(dataPath, "department.csv"));
        var statuses = ReadStatuses(Path.Combine(dataPath, "task_status.csv"));
        var tasks = ReadTasks(Path.Combine(dataPath, "task.csv"));

        var departmentIds = departments.Select(department => department.Id).ToHashSet();
        var statusIds = statuses.Select(status => status.Id).ToHashSet();
        foreach (var task in tasks)
        {
            if (task.DepartmentId is Guid departmentId && !departmentIds.Contains(departmentId))
            {
                throw new InvalidDataException(
                    $"Task '{task.Id}' references unknown department '{departmentId}'.");
            }

            if (task.StatusId is Guid statusId && !statusIds.Contains(statusId))
            {
                throw new InvalidDataException(
                    $"Task '{task.Id}' references unknown status '{statusId}'.");
            }

            if (statuses.Any(status => status.Id == task.StatusId && status.Name == "Скасовано")
                && string.IsNullOrWhiteSpace(task.CancelReason))
            {
                throw new InvalidDataException(
                    $"Cancelled task '{task.Id}' is missing its cancellation reason.");
            }
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Departments.AddRange(departments);
        db.Statuses.AddRange(statuses);
        db.CrmTasks.AddRange(tasks);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Imported {TaskCount} tasks, {DepartmentCount} departments, and {StatusCount} statuses from CSV.",
            tasks.Count,
            departments.Count,
            statuses.Count);
    }

    private static List<Department> ReadDepartments(string path)
    {
        return ReadCsv(path, DepartmentColumns)
            .Select((fields, index) => new Department
            {
                Id = ParseGuid(fields[0], path, index + 2, "id"),
                Name = Required(fields[1], path, index + 2, "name")
            })
            .ToList();
    }

    private static List<Status> ReadStatuses(string path)
    {
        return ReadCsv(path, StatusColumns)
            .Select((fields, index) => new Status
            {
                Id = ParseGuid(fields[0], path, index + 2, "id"),
                Name = Required(fields[1], path, index + 2, "name")
            })
            .ToList();
    }

    private static List<CrmTask> ReadTasks(string path)
    {
        return ReadCsv(path, TaskColumns)
            .Select((fields, index) =>
            {
                var line = index + 2;
                return new CrmTask
                {
                    Id = ParseGuid(fields[0], path, line, "id"),
                    Name = Optional(fields[1]),
                    StatusId = OptionalGuid(fields[2], path, line, "status_id"),
                    DepartmentId = OptionalGuid(fields[3], path, line, "department_id"),
                    Assignee = Optional(fields[4]),
                    Deadline = OptionalDate(fields[5], path, line, "deadline"),
                    Description = Optional(fields[6]),
                    CancelReason = Optional(fields[7])
                };
            })
            .ToList();
    }

    private static IEnumerable<string[]> ReadCsv(string path, string[] expectedColumns)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Required seed CSV was not found: '{path}'.", path);
        }

        using var parser = new TextFieldParser(path);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;

        var header = parser.ReadFields();
        if (header is null || !header.SequenceEqual(expectedColumns, StringComparer.Ordinal))
        {
            throw new InvalidDataException(
                $"CSV '{path}' must have columns: {string.Join(",", expectedColumns)}.");
        }

        var line = 1;
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            line++;
            if (fields is null || fields.Length != expectedColumns.Length)
            {
                throw new InvalidDataException(
                    $"CSV '{path}' has an invalid number of fields near record {line}.");
            }

            yield return fields;
        }
    }

    private static string Required(string value, string path, int line, string column)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"CSV '{path}', record {line}: '{column}' is required.");
        }

        return value;
    }

    private static string? Optional(string value) =>
        string.IsNullOrEmpty(value) ? null : value;

    private static Guid ParseGuid(string value, string path, int line, string column)
    {
        if (!Guid.TryParse(value, out var result))
        {
            throw new InvalidDataException(
                $"CSV '{path}', record {line}: '{column}' must be a UUID.");
        }

        return result;
    }

    private static Guid? OptionalGuid(string value, string path, int line, string column) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseGuid(value, path, line, column);

    private static DateOnly? OptionalDate(string value, string path, int line, string column)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateOnly.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            throw new InvalidDataException(
                $"CSV '{path}', record {line}: '{column}' must use YYYY-MM-DD.");
        }

        return result;
    }
}
