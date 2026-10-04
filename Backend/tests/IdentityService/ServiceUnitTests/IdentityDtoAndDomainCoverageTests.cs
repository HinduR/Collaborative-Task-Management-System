using System.Reflection;
using FluentAssertions;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Domain.Models;
using Xunit;

namespace IdentityService.UnitTests;

public class IdentityDtoAndDomainCoverageTests
{
    public static IEnumerable<object[]> DataTypes()
    {
        IEnumerable<Type> dtoTypes = typeof(AuthenticatedUserDto).Assembly
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => type.Namespace?.StartsWith("IdentityService.Application.", StringComparison.Ordinal) == true)
            .Where(type => type.Name.EndsWith("Dto", StringComparison.Ordinal) ||
                           type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null);

        IEnumerable<Type> domainTypes = typeof(User).Assembly
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => type.Namespace == "IdentityService.Domain.Models")
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null);

        return dtoTypes.Concat(domainTypes).Select(type => new object[] { type });
    }

    [Theory]
    [MemberData(nameof(DataTypes))]
    public void Properties_ShouldRoundTripValues(Type type)
    {
        object instance = Activator.CreateInstance(type)!;

        foreach (PropertyInfo property in type.GetProperties().Where(property => property.CanRead && property.CanWrite))
        {
            object? value = CreateValue(property.PropertyType);

            property.SetValue(instance, value);

            property.GetValue(instance).Should().BeEquivalentTo(value);
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
            return 5;
        }

        if (underlyingType == typeof(DateTime))
        {
            return DateTime.UtcNow;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type itemType = type.GetGenericArguments()[0];
            object list = Activator.CreateInstance(type)!;
            type.GetMethod("Add")!.Invoke(list, [CreateValue(itemType)]);
            return list;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            Type itemType = type.GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(itemType);
            object list = Activator.CreateInstance(listType)!;
            listType.GetMethod("Add")!.Invoke(list, [CreateDomainValue(itemType)]);
            return list;
        }

        return CreateDomainValue(underlyingType);
    }

    private static object? CreateDomainValue(Type type)
    {
        object? value = Activator.CreateInstance(type);

        if (value is not null)
        {
            (type.GetProperty("Name") ?? type.GetProperty("Email") ?? type.GetProperty("RefTermKey"))
                ?.SetValue(value, "value");
        }

        return value;
    }
}
