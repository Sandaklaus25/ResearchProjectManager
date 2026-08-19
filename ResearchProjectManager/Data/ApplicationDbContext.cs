using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Models;

using Attachment = ResearchProjectManager.Models.Attachment;

namespace ResearchProjectManager.Data
{
    // Use the IdentityDbContext overload that accepts the role and key types when your User uses a non-string key
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Team> Teams { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Team>().ToTable("Teams");
            modelBuilder.Entity<UserTeam>().ToTable("UserTeams");
            modelBuilder.Entity<ProjectTask>().ToTable("ProjectTasks");
            modelBuilder.Entity<TaskAssignment>().ToTable("TaskAssignments");
            modelBuilder.Entity<Subtask>().ToTable("Subtasks");
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Attachment>().ToTable("Attachments");

            modelBuilder.Entity<UserTeam>()
                .HasKey(ut => new { ut.UserId, ut.TeamId });

            modelBuilder.Entity<UserTeam>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTeams)
                .HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<UserTeam>()
                .HasOne(ut => ut.Team)
                .WithMany(t => t.UserTeams)
                .HasForeignKey(ut => ut.TeamId);

            //TODO: Add relationships for TaskAssignment, Subtask, Comment, and Attachment as needed
            //TODO: Restrictions on data (ex. max length, required fields, etc.) can be added here as well
        }
    }
}