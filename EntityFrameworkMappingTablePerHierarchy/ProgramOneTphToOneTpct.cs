using System;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class ProgramOneTphToOneTpct
    {
        private enum ParentDiscriminator
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
            protected ParentDiscriminator Discriminator { get; set; }
        }

        private class GoodChild : ChildBase
        {
            public GoodChild()
            {
                Discriminator = ParentDiscriminator.Good;
            }
            public virtual GoodParent Parent { get; set; }

            public virtual string GoodChildData { get; set; }
        }

        private class BadChild : ChildBase
        {
            public BadChild()
            {
                Discriminator = ParentDiscriminator.Bad;
            }

            public virtual BadParent Parent { get; set; }
            public virtual string BadChildData { get; set; }
        }

        public class TestContext : DbContext
        {

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

            }
        }

        private static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-",string.Empty).Substring(0, 30);

        [Fact]
        public static void One_TablePerHierarchy_to_One_TablePerConcreteType()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneTphToOneTpct)};Integrated Security=SSPI;").Options;

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
