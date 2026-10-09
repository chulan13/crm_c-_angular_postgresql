using abcomm_test.Models;
using Microsoft.EntityFrameworkCore;

namespace abcomm_test.Data;
public class CrmDbContext(DbContextOptions<CrmDbContext> options)
    : DbContext(options)
{
    public DbSet<CrmTask> CrmTasks => Set<CrmTask>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Status> Statuses => Set<Status>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrmTask>(entity =>
        {
            entity.Property(task => task.Id).HasColumnName("id");
            entity.Property(task => task.Name).HasColumnName("name");
            entity.Property(task => task.StatusId).HasColumnName("status_id");
            entity.Property(task => task.DepartmentId).HasColumnName("department_id");
            entity.Property(task => task.Assignee).HasColumnName("assignee");
            entity.Property(task => task.Deadline).HasColumnName("deadline");
            entity.Property(task => task.Description).HasColumnName("description");
            entity.Property(task => task.CancelReason)
                .HasColumnName("cancel_reason")
                .HasMaxLength(250);
            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(task => task.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Status>()
                .WithMany()
                .HasForeignKey(task => task.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(department => department.Id).HasColumnName("id");
            entity.Property(department => department.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.Property(status => status.Id).HasColumnName("id");
            entity.Property(status => status.Name).HasColumnName("name");
            entity.HasIndex(status => status.Name).IsUnique();
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Statuses_Name_Allowed",
                "\"name\" IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')"));
        });
    }
}