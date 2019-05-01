using System;
using System.Collections.Generic;
using Xunit;
using static Microsoft.EntityFrameworkCore.TablePerHierarchy.Helper;

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

            public TestContext(DbContextOptions options, bool applyWithColumnTypeBigintToDiscriminator) :
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
                    builder.HasDiscriminator<ParentChildDiscriminator>(nameof(ChildBase.Discriminator))
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

                #region BadChild
                {
                    var builder = modelBuilder.Entity<BadChild>(
                        b => b.Property(nameof(BadChild.BadChildData)).HasColumnName("BadChildData"));
                    modelBuilder.Entity<BadChild>()
                        .HasBaseType<ChildBase>();
                    //builder.Ignore(typeof(ParentChildDiscriminator));
                }
                #endregion

                #region GoodChild
                {
                    var builder = modelBuilder.Entity<GoodChild>(
                        b => b.Property(nameof(GoodChild.GoodChildData)).HasColumnName("GoodChildData"));
                    modelBuilder.Entity<GoodChild>()
                        .HasBaseType<ChildBase>();
                    //builder.Ignore(typeof(ParentChildDiscriminator));
                }
                #endregion
            }
        }

        public static IEnumerable<object[]> ShouldSucceedRegardlessOfTrueOrFalseInput()
        {
            return new List<object[]>() { new object[] {true}, new object[] {false}};
        }

        [Theory]
        [MemberData(nameof(ShouldSucceedRegardlessOfTrueOrFalseInput))]
        public static void One_Child_TablePerHierarchy_SanityCheck(bool applyColumnType)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneChildTphSanityCheck)}_{applyColumnType};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options, applyColumnType))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var goodChild = new GoodChild() {GoodChildData = Random30Characters()};

                db.Add(goodChild);

                var badChild = new BadChild() {};

                db.Add(badChild);

                db.SaveChanges();
            }
        }
    }
}
