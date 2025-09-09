using Humanizer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Peers.Core.Test.Data;

public class SnakeCaseNamingConventionsTests
{
    [Fact]
    public void Rewrites_all_columns_as_snake_case_and_merges_owned_columns()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        // These options will be used by the context instances in this test suite, including the connection opened above.
        var options = new DbContextOptionsBuilder<MyContext>()
            .UseSqlite(conn)
            .Options;

        using var db = new MyContext(options);
        db.Database.EnsureCreated();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "select id, name, street, number, code from company;";
            // Shouldn't throw
            cmd.ExecuteReader();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "select id, name, the_street from person;";
            // Shouldn't throw
            cmd.ExecuteReader();
        }
    }

    public class MyContext : TestContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Person> Persons { get; set; }
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CompanyMapping());
            modelBuilder.ApplyConfiguration(new PersonMapping());
        }
    }

    public class Company
    {
        public int Id { get; set; }
        public Address Address { get; set; }
        public Building Building { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public Address Address { get; set; }
    }

    public record Address
    {
        public string Name { get; init; }
        public string Street { get; init; }
    }

    public record Building
    {
        public string Number { get; init; }
        public string Code { get; init; }
    }

    public class CompanyMapping : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder
                .OwnsOne(p => p.Address, x => x.Property(p => p.Name).IsRequired())
                .Navigation(o => o.Address)
                .IsRequired();

            builder
                .ComplexProperty(p => p.Building)
                .IsRequired();

            builder.ToTable(nameof(Company).Underscore());
        }
    }

    public class PersonMapping : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder
                .OwnsOne(p => p.Address, x =>
                {
                    x.Property(p => p.Name).IsRequired();
                    x.Property(p => p.Street).HasColumnName("the_street").IsRequired();
                })
                .Navigation(o => o.Address)
                .IsRequired();

            builder.ToTable(nameof(Person).Underscore());
        }
    }
}
