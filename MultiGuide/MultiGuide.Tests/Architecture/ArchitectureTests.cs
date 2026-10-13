using Microsoft.AspNetCore.Mvc;
using MultiGuide.Api.Controllers;
using MultiGuide.Business.Services;
using MultiGuide.Domain.Entities;
using System.Reflection;

namespace MultiGuide.Tests.Architecture;

public sealed class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(TourArea).Assembly;
    private static readonly Assembly BusinessAssembly = typeof(IAreaService).Assembly;
    private static readonly Assembly DataAssembly = typeof(MultiGuide.Data.AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AreasController).Assembly;

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherProject()
    {
        var referencedAssemblyNames = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .ToList();

        Assert.DoesNotContain("MultiGuide.Business", referencedAssemblyNames);
        Assert.DoesNotContain("MultiGuide.Data", referencedAssemblyNames);
        Assert.DoesNotContain("MultiGuide.Api", referencedAssemblyNames);
    }

    [Fact]
    public void Business_ShouldOnlyReferenceDomain_AndNotDataOrApiOrEfCore()
    {
        var referencedAssemblyNames = BusinessAssembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .ToList();

        // Business không được tham chiếu Data, Api, EF Core
        Assert.DoesNotContain("MultiGuide.Data", referencedAssemblyNames);
        Assert.DoesNotContain("MultiGuide.Api", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", referencedAssemblyNames);
        Assert.DoesNotContain("MySql.EntityFrameworkCore", referencedAssemblyNames);

        // Business PHẢI tham chiếu Domain
        Assert.Contains("MultiGuide.Domain", referencedAssemblyNames);
    }

    [Fact]
    public void Data_ShouldNotReferenceApi()
    {
        var referencedAssemblyNames = DataAssembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .ToList();

        Assert.DoesNotContain("MultiGuide.Api", referencedAssemblyNames);
    }

    [Fact]
    public void Controllers_ShouldNotDirectlyReferenceDbContext()
    {
        var controllerTypes = ApiAssembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        foreach (var controller in controllerTypes)
        {
            var constructors = controller.GetConstructors();
            foreach (var ctor in constructors)
            {
                var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType);
                Assert.DoesNotContain(typeof(MultiGuide.Data.AppDbContext), parameterTypes);
            }
        }
    }
}
