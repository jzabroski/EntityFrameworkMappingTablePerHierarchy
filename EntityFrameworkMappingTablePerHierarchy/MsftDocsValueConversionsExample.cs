using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    /// <summary>
    /// These tests are based on: https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#configuring-a-value-converter
    /// </summary>
    public class MsftDocsValueConversionExample
    {
        public enum ConversionStrategy
        {
            None,
            TypeParameter,
            BuiltInNumberConverter,
            BuiltInStringConverter,
            FluentInlineNumberConverter,
            FluentInlineStringConverter
        }
        public class Rider
        {
            public int Id { get; set; }
            public EquineBeast Mount { get; set; }
        }

        public enum EquineBeast
        {
            Donkey,
            Mule,
            Horse,
            Unicorn
        }

        public class TestContext : DbContext
        {
            private readonly ConversionStrategy _conversionStrategy;
            private readonly bool _applyColumnType;

            public TestContext(DbContextOptions options, ConversionStrategy conversionStrategy, bool applyColumnType) :
                base(options)
            {
                _conversionStrategy = conversionStrategy;
                _applyColumnType = applyColumnType;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

                base.OnModelCreating(modelBuilder);

                switch (_conversionStrategy)
                {
                    case ConversionStrategy.None:
                    {
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount);
                        if (_applyColumnType)
                            enumProperty.HasColumnType("bigint");
                    }
                        break;
                    case ConversionStrategy.TypeParameter:
                    {
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount)
                            .HasConversion<long>();
                        if (_applyColumnType)
                            enumProperty.HasColumnType("bigint");
                    }
                        break;
                    case ConversionStrategy.BuiltInNumberConverter:
                    {
                        var converter = new EnumToNumberConverter<EquineBeast, short>();
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount)
                            .HasConversion(converter);
                        if (_applyColumnType)
                            enumProperty.HasColumnType("bigint");
                    }
                        break;
                    case ConversionStrategy.BuiltInStringConverter:
                    {
                        var converter = new EnumToStringConverter<EquineBeast>();
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount)
                            .HasConversion(converter);
                        if (_applyColumnType)
                            enumProperty.HasColumnType("varchar(20)");
                    }
                        break;
                    case ConversionStrategy.FluentInlineNumberConverter:
                    {
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount)
                            .HasConversion(
                                v => (long)v,
                                v => (EquineBeast)v);
                        if (_applyColumnType)
                            enumProperty.HasColumnType("bigint");
                    }
                        break;
                    case ConversionStrategy.FluentInlineStringConverter:
                    {
                        var enumProperty = modelBuilder
                            .Entity<Rider>()
                            .Property(e => e.Mount)
                            .HasConversion(
                                v => v.ToString(),
                                v => (EquineBeast)Enum.Parse(typeof(EquineBeast), v));
                        if (_applyColumnType)
                            enumProperty.HasColumnType("varchar(20)");
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_conversionStrategy));
                }
            }
        }

        public static IEnumerable<object[]> ShouldSucceedRegardlessOfConversionStrategy()
        {
            return new List<object[]>()
            {
                new object[] {ConversionStrategy.None, true},
                new object[] {ConversionStrategy.None, false},
                new object[] {ConversionStrategy.TypeParameter, true},
                new object[] {ConversionStrategy.TypeParameter, false},
                new object[] {ConversionStrategy.BuiltInNumberConverter, true},
                new object[] {ConversionStrategy.BuiltInNumberConverter, false},
                new object[] {ConversionStrategy.BuiltInStringConverter, true},
                new object[] {ConversionStrategy.BuiltInStringConverter, false},
                new object[] {ConversionStrategy.FluentInlineNumberConverter, true},
                new object[] {ConversionStrategy.FluentInlineNumberConverter, false},
                new object[] {ConversionStrategy.FluentInlineStringConverter, true},
                new object[] {ConversionStrategy.FluentInlineStringConverter, false}
            };
        }

        [Theory]
        [MemberData(nameof(ShouldSucceedRegardlessOfConversionStrategy))]
        public static void Should_not_throw_InvalidOperationException_when_using_enum_as_discriminator(ConversionStrategy conversionStrategy, bool applyColumnType)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(MsftDocsValueConversionExample)}_{conversionStrategy}_{applyColumnType};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options, conversionStrategy, applyColumnType))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var rider = new Rider()
                {
                    Mount = EquineBeast.Horse
                };

                db.Add(rider);

                db.SaveChanges();

                var sameRider = db.Set<Rider>().FirstOrDefault(r => r.Id == rider.Id);
                Assert.True(sameRider != null);
                var sameRider2 = db.Set<Rider>().FirstOrDefault(r => r.Mount == EquineBeast.Horse);
                Assert.True(sameRider2 != null);
            }
        }
    }
}
