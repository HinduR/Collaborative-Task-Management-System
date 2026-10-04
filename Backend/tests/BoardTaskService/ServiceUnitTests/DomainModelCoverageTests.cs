using System.Reflection;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Xunit;

namespace BoardTaskService.UnitTests;

public class DomainModelCoverageTests
{
    public static IEnumerable<object[]> ModelTypes()
    {
        return typeof(Board).Assembly
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => type.Namespace == "BoardTaskService.Domain.Models")
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null)
            .Select(type => new object[] { type });
    }

    [Theory]
    [MemberData(nameof(ModelTypes))]
    public void ModelProperties_ShouldRoundTripValues(Type modelType)
    {
        object model = Activator.CreateInstance(modelType)!;

        foreach (PropertyInfo property in modelType.GetProperties().Where(property => property.CanRead && property.CanWrite))
        {
            object? value = CreateValue(property.PropertyType);

            property.SetValue(model, value);

            property.GetValue(model).Should().BeEquivalentTo(value);
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
            return 3;
        }

        if (underlyingType == typeof(DateTime))
        {
            return DateTime.UtcNow;
        }

        if (type.IsArray && type.GetElementType() == typeof(Guid))
        {
            return new[] { Guid.NewGuid() };
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            Type itemType = type.GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(itemType);
            object list = Activator.CreateInstance(listType)!;
            object? item = CreateNavigationValue(itemType);

            listType.GetMethod("Add")!.Invoke(list, [item]);
            return list;
        }

        return CreateNavigationValue(underlyingType);
    }

    private static object? CreateNavigationValue(Type type)
    {
        if (type.Namespace != "BoardTaskService.Domain.Models")
        {
            return Activator.CreateInstance(type);
        }

        object model = Activator.CreateInstance(type)!;
        PropertyInfo? requiredName = type.GetProperty("Name") ?? type.GetProperty("Title") ?? type.GetProperty("Comment");
        requiredName?.SetValue(model, "value");
        return model;
    }
}
