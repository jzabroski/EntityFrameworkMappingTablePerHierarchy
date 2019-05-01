# EntityFrameworkMappingTablePerHierarchy

This is an attempt to wrap my head around EntityFrameworkCore's Table-Per-Hierarchy (TPH) implementation, and suss out any issues in it.

Core concerns are:
- In March 2018, EntityFrameworkCore team [fixed mapping Discriminator as an enum](https://github.com/aspnet/EntityFrameworkCore/issues/11454).  This does not appear to work using fluent syntax.  The [pull request associated with the fix](https://github.com/aspnet/EntityFrameworkCore/commit/e6894d01230abec664b7461aa76464edeaada3e0) seems to be more of a unit test than integration test.
- Discriminator usage is finicky, in several ways:
    - `HasDiscriminator` is finicky.
        - WON'T WORK: `HasDiscriminator<TEntity>(x => x.DiscriminatorEnum)`
            - TODO: Suggest EFCore Roslyn Analyzer to detect this issue.
        - WILL WORK: `HasDiscriminator(x => x.DiscriminatorEnum)`
        - WILL WORK: `HasDiscriminator<TEntity>(nameof(TEntity.DiscriminatorEnum))`
    - You cannot combine `discriminator.HasColumnType("bigint")` with `discriminator.HasConversion(v => v.ToString(), v => (ChildDiscriminator)Enum.Parse(typeof(ChildDiscriminator), v));`
        - Instead, use: `builder.Property(x => x.ChildDiscriminator).HasConversion<long>().HasColumnType("BIGINT");`
        - Doing so will yield the following error:
        ```
        System.InvalidOperationException : The property 'BadChild.ChildDiscriminator' is of type 'ParentChildDiscriminator' which is not supported by current database provider. Either change the property CLR type or ignore the property using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
           at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.PropertyMappingValidationConvention.Apply(InternalModelBuilder modelBuilder)
           at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ImmediateConventionScope.OnModelBuilt(InternalModelBuilder modelBuilder)
           at Microsoft.EntityFrameworkCore.ModelBuilder.FinalizeModel()
           at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
           at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)
           at System.Lazy`1.CreateValue()
           at Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel()
           at Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
           at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
           at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
           at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
           at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
           at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
           at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
           at Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
           at Microsoft.EntityFrameworkCore.Internal.InternalAccessorExtensions.GetService[TService](IInfrastructure`1 accessor)
           at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.get_DatabaseCreator()
           at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureDeleted()
           at Microsoft.EntityFrameworkCore.TablePerHierarchy.ProgramOneTphToOneTph.One_TablePerHierarchy_To_One_TablePerHierarchy(Boolean applyColumnType) in D:\source\GitHub\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\ProgramOneTphToOneTph.cs:line 251
        ```

- Can I [reference a TPH twice in the same entity](https://github.com/aspnet/EntityFrameworkCore/issues/5001)?
- Can I have One TPH with a foreign key to one TPH leaf entity?
- Why are all my exceptions occurring on `IDatabaseCreator.EnsureDeleted`?

# Findings So Far

1. `System.InvalidOperationException` messages related to type mapping seem to be raised in the order of lexicographic name of the type.  It's probably the full name and not just the short name.
2. Testing mappings appears to be non-thread-safe.  That is, if you register the same type in two different context instances, even if those contexts connect to two separate databases, the component internal to EF called the "snapshot generator" appears to be a global variable of sorts. - Need to better understand exactly what is going on here.
3. The documentation on adding a ValueConverter for an enum appears to be incorrect (or I am typing in the example wrong when translating it to my own sample): https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#configuring-a-value-converter

# Domain Model

This project started off as an attempt to model a specific domain model using a non-trivial Table-Per-Hierarchy.

## C# Class Diagram

```
    .--------------------------------------------.     .----------------------------------------------.
    |  ParentBase <abstract>                     |     | ChildBase <abstract>                         |
    +--------------------------------------------+     +----------------------------------------------+
    | Id : long                                  |     | Id : long                                    |
    '--------------------------------------------'     '----------------------------------------------'
        |                                     |            |                                      |
       \|/                                   \|/          \|/                                    \|/
.-------------------------. .------------------------. .------------------------. .-----------------------.
| GoodParent              | | BadParent              | | GoodChild              | | BadChild              |
+-------------------------+ +------------------------+ +------------------------+ +-----------------------+
| Child : GoodChild       | | Child : BadChild       | | Parent : GoodParent    | | Parent : BadParent    |
| GoodParentData : string | | BadParentData : string | | GoodChildData : string | | BadChildData : string |
'-------------------------' '------------------------' '------------------------' '-----------------------'
```

## Sql Schema

```
.--------------------------------.
|  Parent                        |
+--------------------------------+
| ParentID       : bigint        |
| Discriminator  : bigint        |
| GoodParentData : varchar       |
| BadParentData  : varchar       |
'--------------------------------'
                | 1
                |
                | 0..1
.--------------------------------.
|  Child                         |
+--------------------------------+
| ChildId        : bigint        |
| ParentId       : bigint        |
| Discriminator  : bigint        |
| GoodChildData  : varchar       |
| BadChildData   : varchar       |
'--------------------------------'                                                         
```

