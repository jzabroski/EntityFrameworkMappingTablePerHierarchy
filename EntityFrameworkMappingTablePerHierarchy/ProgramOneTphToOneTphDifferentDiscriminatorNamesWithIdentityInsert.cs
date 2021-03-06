﻿using Xunit;
using static Microsoft.EntityFrameworkCore.TablePerHierarchy.Helper;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class ProgramOneTphToOneTphDifferentDiscriminatorNamesWithIdentityInsert
    {
        private enum ParentDiscriminator : long
        {
            Good = 1,
            Bad = 2
        }
        private enum ParentChildDiscriminator : long
        {
            Good = 1,
            Bad = 2
        }
        private abstract class EntityBaseBase { }
        private abstract class EntityBase : EntityBaseBase
        {
            public virtual long Id { get; set; }
        }
        private abstract class ParentBase : EntityBase
        {
            //public virtual long Id { get; set; }

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

        private abstract class ChildBase : EntityBase
        {
            //public virtual long Id { get; set; }
            public ParentChildDiscriminator ChildDiscriminator { get; protected set; }
        }

        private class GoodChild : ChildBase
        {
            public GoodChild()
            {
                ChildDiscriminator = ParentChildDiscriminator.Good;
            }
            public virtual GoodParent Parent { get; set; }

            public virtual string GoodChildData { get; set; }
        }

        private class BadChild : ChildBase
        {
            public BadChild()
            {
                ChildDiscriminator = ParentChildDiscriminator.Bad;
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
                    builder.Property<long>(x => x.Id).UseSqlServerIdentityColumn<long>().ValueGeneratedOnAdd();
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
                    builder.Property<long>(x => x.Id).UseSqlServerIdentityColumn<long>().ValueGeneratedOnAdd();
                    builder.HasDiscriminator(x => x.ChildDiscriminator)
                        .HasValue<GoodChild>(ParentChildDiscriminator.Good)
                        .HasValue<BadChild>(ParentChildDiscriminator.Bad);
                }
                #endregion

                #region GoodChild
                {
                    var builder = modelBuilder.Entity<GoodChild>();
                    builder
                        .HasBaseType<ChildBase>();
                    builder.Property<long>("ParentId").HasColumnName("ParentId");
                    builder
                        .HasOne(e => e.Parent)
                        .WithOne(e => e.Child)
                        .HasForeignKey<GoodChild>("ParentId").OnDelete(DeleteBehavior.ClientSetNull);
                }
                #endregion

                #region BadChild
                {
                    var builder = modelBuilder.Entity<BadChild>();
                    builder
                        .HasBaseType<ChildBase>();
                    builder.Property<long>("ParentId").HasColumnName("ParentId");
                    builder
                        .HasOne(e => e.Parent)
                        .WithOne(e => e.Child)
                        .HasForeignKey<BadChild>("ParentId").OnDelete(DeleteBehavior.ClientSetNull);
                }
                #endregion
            }
        }

        [Fact]
        public static void One_TablePerHierarchy_To_One_TablePerHierarchy_Different_Discriminator_Names_With_Identity_Insert()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(ProgramOneTphToOneTphDifferentDiscriminatorNamesWithIdentityInsert)};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var goodChild = new GoodChild() { Parent = new GoodParent() { }, GoodChildData = Random30Characters() };

                db.Add(goodChild);

                var badChild = new BadChild() { Parent = new BadParent() { }, BadChildData = Random30Characters() };

                db.Add(badChild);

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
