using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Z.EntityFramework.Extensions;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    /// <summary>
    /// This test generates a PropertyWrongClrType message.
    /// See: https://github.com/aspnet/EntityFrameworkCore/blob/ed0cf26ad6fc200a47a803999e701ada1ce1101a/src/EFCore/Properties/CoreStrings.resx#L421
    /// The error appears to be thrown from:
    /// https://github.com/aspnet/EntityFrameworkCore/blob/1fef013c122da8e18c121f7713337c0449d2baee/src/EFCore/Metadata/Internal/EntityType.cs#L1585
    /// The problem might be this line of code:
    /// <code>ClrType?.GetMembersInHierarchy(name).FirstOrDefault(),</code>
    /// And the implementation can be found here:
    /// https://github.com/aspnet/EntityFrameworkCore/blob/43f040f510450cb4ba25e4690b0ddac88e1019ee/src/Shared/SharedTypeExtensions.cs#L256
    /// </summary>
    /// <remarks>
    /// The following error message is generated:
    /// 
    /// System.InvalidOperationException : The property 'Discriminator' cannot be added to type 'ChildBase' because the type of the corresponding CLR property or field 'ParentChildDiscriminator' does not match the specified type 'string'.
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.EntityType.AddProperty(String name, Type propertyType, MemberInfo memberInfo, ConfigurationSource configurationSource, Nullable`1 typeConfigurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.EntityType.AddProperty(String name, Type propertyType, ConfigurationSource configurationSource, Nullable`1 typeConfigurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalEntityTypeBuilder.Property(Property existingProperty, String propertyName, Type propertyType, MemberInfo clrProperty, Nullable`1 configurationSource, Nullable`1 typeConfigurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalEntityTypeBuilder.Property(String propertyName, Type propertyType, MemberInfo memberInfo, Nullable`1 configurationSource, Nullable`1 typeConfigurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalEntityTypeBuilder.Property(String propertyName, Type propertyType, ConfigurationSource configurationSource, Nullable`1 typeConfigurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalEntityTypeBuilder.Property(String propertyName, Type propertyType, ConfigurationSource configurationSource)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.RelationalEntityTypeBuilderAnnotations.DiscriminatorBuilder(Func`2 createProperty, Type propertyType)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.DiscriminatorConvention.Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ImmediateConventionScope.OnBaseEntityTypeChanged(InternalEntityTypeBuilder entityTypeBuilder, EntityType previousBaseType)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.RunVisitor.VisitOnBaseEntityTypeChanged(OnBaseEntityTypeChangedNode node)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ConventionVisitor.VisitConventionScope(ConventionScope node)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ConventionVisitor.VisitConventionScope(ConventionScope node)
    /// at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ConventionBatch.Run()
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalModelBuilder.Entity(TypeIdentity&amp; type, ConfigurationSource configurationSource, Boolean allowOwned, Boolean throwOnQuery)
    /// at Microsoft.EntityFrameworkCore.Metadata.Internal.InternalModelBuilder.Entity(Type type, ConfigurationSource configurationSource, Boolean allowOwned, Boolean throwOnQuery)
    /// at Microsoft.EntityFrameworkCore.ModelBuilder.Entity[TEntity]()
    /// at Microsoft.EntityFrameworkCore.TablePerHierarchy.ProgramOneTphToOneTph.TestContext.OnModelCreating(ModelBuilder modelBuilder) in D:\source\GitHub\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\ProgramOneTphToOneTph.cs:line 131
    /// at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
    /// at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
    /// at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)
    /// at System.Lazy`1.CreateValue()
    /// at Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel()
    /// at Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
    /// at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
    /// at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
    /// at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
    /// at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
    /// at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
    /// at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
    /// at Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
    /// at Microsoft.EntityFrameworkCore.Internal.InternalAccessorExtensions.GetService[TService](IInfrastructure`1 accessor)
    /// at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.get_DatabaseCreator()
    /// at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureDeleted()
    /// at Microsoft.EntityFrameworkCore.TablePerHierarchy.ProgramOneTphToOneTph.One_TablePerHierarchy_To_One_TablePerHierarchy() in D:\source\GitHub\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\EntityFrameworkMappingTablePerHierarchy\ProgramOneTphToOneTph.cs:line 183

    /// </remarks>
    public class ProgramOneTphToOneTph
    {
        private enum ParentDiscriminator
        {
            Good = 1,
            Bad = 2
        }
        private enum ParentChildDiscriminator
        {
            Good = 1,
            Bad = 2
        }
        private abstract class ParentBase
        {
            public virtual int Id { get; set; }

            public ParentDiscriminator Discriminator { get; protected set; }
        }

        private class GoodParent : ParentBase
        {
            public GoodParent()
            {
                Discriminator = ParentDiscriminator.Good;
            }

            public virtual GoodChild Child { get; set; }
            public virtual string GoodParentData { get; set; }
        }

        private class BadParent : ParentBase
        {
            public BadParent()
            {
                Discriminator = ParentDiscriminator.Bad;
            }
            public virtual BadChild Child { get; set; }
            public virtual string BadParentData { get; set; }
        }

        private abstract class ChildBase
        {
            public virtual int Id { get; set; }
            public ParentChildDiscriminator Discriminator { get; protected set; }
        }

        private class GoodChild : ChildBase
        {
            public GoodChild()
            {
                Discriminator = ParentChildDiscriminator.Good;
            }
            public virtual GoodParent Parent { get; set; }

            public virtual string GoodChildData { get; set; }
        }

        private class BadChild : ChildBase
        {
            public BadChild()
            {
                Discriminator = ParentChildDiscriminator.Bad;
            }

            public virtual BadParent Parent { get; set; }
            public virtual string BadChildData { get; set; }
        }

        public class TestContext : DbContext
        {
            static TestContext() { EntityFrameworkManager.ContextFactory = (c) => c; }

            public TestContext(DbContextOptions options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                #region ParentBase
                {
                    var builder = modelBuilder.Entity<ParentBase>();
                    builder.HasDiscriminator(x => x.Discriminator)
                        .HasValue<GoodParent>(ParentDiscriminator.Good)
                        .HasValue<BadParent>(ParentDiscriminator.Bad);

                    //builder.Property(x => x.Discriminator)
                    //    .HasColumnName("DiscriminatorId");
                }
                #endregion

                #region GoodParent
                {
                    var builder = modelBuilder.Entity<GoodParent>();
                    builder
                        .HasBaseType<ParentBase>();
                    builder
                        .HasOne(e => e.Child)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<GoodChild>("ParentId");
                }
                #endregion

                #region BadParent
                {
                    var builder = modelBuilder.Entity<BadParent>();
                    builder
                        .HasBaseType<ParentBase>();
                    builder.HasOne(e => e.Child)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<BadChild>("ParentId");
                }
                #endregion

                #region ChildBase
                {
                    var builder = modelBuilder.Entity<ChildBase>();
                    builder.HasDiscriminator(x => x.Discriminator)
                        .HasValue<GoodChild>(ParentChildDiscriminator.Good)
                        .HasValue<BadChild>(ParentChildDiscriminator.Bad);

                    // Based on: https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#configuring-a-value-converter
                    builder.Property(e => e.Discriminator).HasColumnType("bigint")
                        .HasConversion(
                            v => v.ToString(),
                            v => (ParentChildDiscriminator) Enum.Parse(typeof(ParentChildDiscriminator), v));

                    // HACK
                    //builder.Property(x => x.Discriminator).HasConversion<long>().HasColumnType("BIGINT"); 
                }
                #endregion

                #region GoodChild
                {
                    var builder = modelBuilder.Entity<GoodChild>();
                    builder
                        .HasBaseType<ChildBase>();
                    builder
                        .HasOne(e => e.Parent)
                        .WithOne(e => e.Child)
                        .HasForeignKey<GoodChild>("ChildId");
                }
                #endregion

                #region BadChild
                {
                    var builder = modelBuilder.Entity<BadChild>();
                    builder
                        .HasBaseType<ChildBase>();
                    builder
                        .HasOne(e => e.Parent)
                        .WithOne(e => e.Child)
                        .HasForeignKey<BadChild>("ChildId");
                }
                #endregion
            }
        }

        private static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 30);

        [Fact]
        public static void One_TablePerHierarchy_To_One_TablePerHierarchy()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneTphToOneTph)};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var goodParent = new GoodParent()
                {
                    Child = new GoodChild() { GoodChildData = Random30Characters() },
                    GoodParentData = Random30Characters()
                };

                db.Add(goodParent);

                var badParent = new BadParent()
                {
                    Child = new BadChild() { BadChildData = Random30Characters() },
                    BadParentData = Random30Characters()
                };

                db.Add(badParent);

                db.SaveChanges();
            }
        }
    }
}
