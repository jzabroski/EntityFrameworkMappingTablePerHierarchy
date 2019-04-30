using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    public class GH11454
    {
        private enum Direction
        {
            Left,
            Right
        }
        private abstract class Base
        {
            [Key]
            public int Id { get; set; }

            public Direction Direction { get; set; }
        }

        private class Left : Base
        {
            public int Number { get; set; }
        }

        private class Right : Base
        {
            public int Number { get; set; }
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

                modelBuilder.Entity<Base>(builder =>
                {
                    builder.HasDiscriminator<Direction>(nameof(Base.Direction))
                        .HasValue<Left>(Direction.Left)
                        .HasValue<Right>(Direction.Right);
                });

                modelBuilder.Entity<Left>(builder =>
                {
                    builder.Property(l => l.Number).HasColumnName(nameof(Left.Number));
                });

                modelBuilder.Entity<Right>(builder =>
                {
                    builder.Property(l => l.Number).HasColumnName(nameof(Right.Number));
                });
            }
        }

        private static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 30);

        public static IEnumerable<object[]> ShouldSucceedRegardlessOfTrueOrFalseInput()
        {
            return new List<object[]>() { new object[] {true}, new object[] {false}};
        }

        [Theory]
        [MemberData(nameof(ShouldSucceedRegardlessOfTrueOrFalseInput))]
        public static void Should_not_throw_InvalidOperationException_when_using_enum_as_discriminator(bool applyColumnType)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer($"Data Source=(local);Initial Catalog=Test_{nameof(GH11454)}_{applyColumnType};Integrated Security=SSPI;").Options;

            using (var db = new TestContext(options, applyColumnType))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // TODO write save logic
                var left = new Left() {};

                db.Add(left);

                var right = new Right() {};

                db.Add(right);

                db.SaveChanges();
            }
        }
    }
}
