using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class GH15666_Explicit
    {
        private class Counterparty
        {
            public virtual long Id { get; set; }
            public virtual string Name { get; set; }
            public virtual  FollowUpInteraction FollowUpInteraction { get; set; }
        }
        private class FollowUpInteraction
        {
            public virtual long Id { get; set; }
            public virtual Counterparty InteractedWithCounterparty { get; set; }
        }
        public class TestContext : DbContext
        {

            public TestContext(DbContextOptions options) :
                base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Counterparty>(builder =>
                {
                    builder.ToTable("Counterparty", "dbo");
                    builder.HasKey(x => x.Id);
                    builder.Property(x => x.Id).HasColumnName("CounterpartyId");
                    builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(25);
                });

                modelBuilder.Entity<FollowUpInteraction>(builder =>
                {
                    builder.ToTable("FollowUpInteraction", "dbo");
                    builder.HasKey(x => x.Id);
                    builder.Property(x => x.Id).HasColumnName("FollowUpInteractionId");
                    builder.HasOne(x => x.InteractedWithCounterparty)
                        .WithOne(x => x.FollowUpInteraction) // missing a relationship here ruins the mapping
                        .HasForeignKey<FollowUpInteraction>("InteractedCounterPartyId");
                });
            }
        }

        private static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 30);

        public static IEnumerable<object[]> ShouldSucceedRegardlessOfTrueOrFalseInput()
        {
            return new List<object[]>() { new object[] {true}, new object[] {false}};
        }

        [Fact]
        public static void Should_not_throw_DbUpdateException()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(GH15666_Explicit)};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var counterparty = new Counterparty() {Name = "Foo"};

                db.Add(counterparty);

                db.SaveChanges();

                var left = new FollowUpInteraction()
                {
                    InteractedWithCounterparty = counterparty
                };

                db.Add(left);

                db.SaveChanges();
            }
        }
    }
}
