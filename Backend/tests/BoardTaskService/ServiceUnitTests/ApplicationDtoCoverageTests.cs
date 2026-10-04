using System.Reflection;
using BoardTaskService.Application.BoardModule.Dto;
using FluentAssertions;
using Xunit;

namespace BoardTaskService.UnitTests;

public class ApplicationDtoCoverageTests
{
    public static IEnumerable<object[]> DtoTypes()
    {
        return typeof(BoardSummaryDto).Assembly
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => type.Namespace?.StartsWith("BoardTaskService.Application.", StringComparison.Ordinal) == true)
            .Where(type => type.Name.EndsWith("Dto", StringComparison.Ordinal) ||
                           type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null)
            .Select(type => new object[] { type });
    }

    [Theory]
    [MemberData(nameof(DtoTypes))]
    public void DtoProperties_ShouldRoundTripValues(Type dtoType)
    {
        object dto = Activator.CreateInstance(dtoType)!;

        foreach (PropertyInfo property in dtoType.GetProperties().Where(property => property.CanRead && property.CanWrite))
        {
            object? value = CreateValue(property.PropertyType);

            property.SetValue(dto, value);

            property.GetValue(dto).Should().BeEquivalentTo(value);
        }
    }

    private static object? CreateValue(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
        {
            return "value";
        }

        if (underlyingType == typeof(Guid))
        {
            return Guid.NewGuid();
        }

        if (underlyingType == typeof(bool))
        {
            return true;
        }

        if (underlyingType == typeof(int))
        {
            return 7;
        }

        if (underlyingType == typeof(DateTime))
        {
            return DateTime.UtcNow;
        }

        if (type.IsArray && type.GetElementType() == typeof(Guid))
        {
            return new[] { Guid.NewGuid() };
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type itemType = type.GetGenericArguments()[0];
            object list = Activator.CreateInstance(type)!;
            object? item = itemType == typeof(Guid)
                ? Guid.NewGuid()
                : Activator.CreateInstance(itemType);

            type.GetMethod("Add")!.Invoke(list, [item]);
            return list;
        }

        return Activator.CreateInstance(underlyingType);
    }
}
