using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Z.EntityFramework.Extensions;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class ProgramOneChildTphSanityCheck
    {
        private enum ParentChildDiscriminator
        {
            Good = 1,
            Bad = 2
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

            public virtual string GoodChildData { get; set; }
        }

        private class BadChild : ChildBase
        {
            public BadChild()
            {
                Discriminator = ParentChildDiscriminator.Bad;
            }
            public virtual string BadChildData { get; set; }
        }

        public class TestContext : DbContext
        {
            private readonly bool _applyWithColumnTypeBigintToDiscriminator;
            //static TestContext() { EntityFrameworkManager.ContextFactory = (c) => c; }

            public TestContext(DbContextOptions options, bool applyWithColumnTypeBigintToDiscriminator = false) :
                base(options)
            {
                _applyWithColumnTypeBigintToDiscriminator = applyWithColumnTypeBigintToDiscriminator;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                #region ChildBase
                {
                    var builder = modelBuilder.Entity<ChildBase>();
                    builder.HasDiscriminator(x => x.Discriminator)
                        .HasValue<GoodChild>(ParentChildDiscriminator.Good)
                        .HasValue<BadChild>(ParentChildDiscriminator.Bad);

                    // Based on: https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#configuring-a-value-converter
                    var discriminator = builder.Property(e => e.Discriminator);

                    if (_applyWithColumnTypeBigintToDiscriminator)
                    {
                        discriminator = discriminator.HasColumnType("bigint");
                    }
                    discriminator.HasConversion(
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
                }
                #endregion

                #region BadChild
                {
                    var builder = modelBuilder.Entity<BadChild>();
                    builder
                        .HasBaseType<ChildBase>();
                }
                #endregion
            }
        }

        private static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 30);

        public static IEnumerable<object[]> ShouldBeTrueRegardlessOfTrueOrFalseInput()
        {
            return new List<object[]>() { new object[] {true}, new object[] {false}};
        }

        [Theory]
        [MemberData(nameof(ShouldBeTrueRegardlessOfTrueOrFalseInput))]
        public static void One_Child_TablePerHierarchy_SanityCheck(bool applyColumnType)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneChildTphSanityCheck)};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options, applyColumnType))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var goodChild = new GoodChild() {GoodChildData = Random30Characters()};

                db.Add(goodChild);

                var badChild = new BadChild() {BadChildData = Random30Characters()};

                db.Add(badChild);

                db.SaveChanges();
            }
        }
    }
}
