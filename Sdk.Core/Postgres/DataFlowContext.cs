using Microsoft.EntityFrameworkCore;
using Sdk.Core.Domain;

namespace Sdk.Core.Postgres;

public class DataFlowContext : DbContext
{
    public DataFlowContext(DbContextOptions<DataFlowContext> options)
        : base(options)
    {
    }

    public DbSet<DataFlow> DataFlows { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataFlow>()
            .HasKey(df => df.Id);

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Id)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.State)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.CreatedAt)
            .IsRequired();

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.UpdatedAt)
            .IsRequired();
        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Source)
            .HasColumnType("jsonb");

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.Destination)
            .HasColumnType("jsonb");

        modelBuilder.Entity<DataFlow>()
            .Property(df => df.TransferType)
            .HasColumnType("jsonb");
    }

    public async Task EnsureMigrated()
    {
        await Database.MigrateAsync();
    }
}