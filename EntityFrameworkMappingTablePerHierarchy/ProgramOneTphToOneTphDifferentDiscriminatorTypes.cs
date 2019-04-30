using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Z.EntityFramework.Extensions;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class ProgramOneTphToOneTphDifferentDiscriminatorTypes
    {
        private enum ParentDiscriminator
        {
            Good = 1,
            Bad = 2
        }
        //private enum ParentChildDiscriminator // use same enum for both ParentBase and ChildBase
        //{
        //    Good = 1,
        //    Bad = 2
        //}
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
            public ParentDiscriminator ChildDiscriminator { get; protected set; }
        }

        private class GoodChild : ChildBase
        {
            public GoodChild()
            {
                ChildDiscriminator = ParentDiscriminator.Good;
            }
            public virtual GoodParent Parent { get; set; }

            public virtual string GoodChildData { get; set; }
        }

        private class BadChild : ChildBase
        {
            public BadChild()
            {
                ChildDiscriminator = ParentDiscriminator.Bad;
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
                    builder.HasDiscriminator(x => x.ChildDiscriminator)
                        .HasValue<GoodChild>(ParentDiscriminator.Good)
                        .HasValue<BadChild>(ParentDiscriminator.Bad);

                   // builder.Property(x => x.ChildDiscriminator).HasConversion<long>().HasColumnType("BIGINT"); // HACK
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
        public static void One_TablePerHierarchy_To_One_TablePerHierarchy_Different_Discriminator_Types()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneTphToOneTphDifferentDiscriminatorTypes)};Integrated Security=SSPI;").Options;

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
