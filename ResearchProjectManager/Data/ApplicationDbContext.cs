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
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseMembers> CourseMembers { get; set; }
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
            
            modelBuilder.Entity<Course>().ToTable("Courses");
            modelBuilder.Entity<CourseMembers>().ToTable("CourseMembers");
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

            modelBuilder.Entity<CourseMembers>()
                .HasKey(cm => new { cm.UserId, cm.CourseId });

            modelBuilder.Entity<CourseMembers>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.EnrolledIn)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseMembers>()
                .HasOne(ut => ut.Course)
                .WithMany(c => c.Members)
                .HasForeignKey(ut => ut.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1. ProjectTask STRICTLY belongs to Course.
            // If the Course is deleted, destroy all tasks inside it.
            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Course)
                .WithMany(c => c.Tasks)
                .HasForeignKey(pt => pt.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Keeping Cascade here because it's a child of Course

            // 2. Break the cascade loop here on the Author!
            // If a user is deleted, do NOT delete the task from the course.
            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Author)
                .WithMany()
                .HasForeignKey(pt => pt.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // This fixes Error 1785!

            // 1. Break loop for Attachments
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attachments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Break loop for Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Break loop for Subtasks
            modelBuilder.Entity<Subtask>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subtasks)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            //TODO: Add relationships for TaskAssignment, Subtask, Comment, and Attachment as needed
            //TODO: Restrictions on data (ex. max length, required fields, etc.) can be added here as well
        }
    }
}