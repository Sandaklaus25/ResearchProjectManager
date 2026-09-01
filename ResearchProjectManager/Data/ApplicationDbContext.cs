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
        public DbSet<CourseMember> CourseMembers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<CourseBan> CourseBans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Course>().ToTable("Courses");
            modelBuilder.Entity<CourseMember>().ToTable("CourseMembers");
            modelBuilder.Entity<Team>().ToTable("Teams");
            modelBuilder.Entity<UserTeam>().ToTable("UserTeams");
            modelBuilder.Entity<ProjectTask>().ToTable("ProjectTasks");
            modelBuilder.Entity<TaskAssignment>().ToTable("TaskAssignments");
            modelBuilder.Entity<Subtask>().ToTable("Subtasks");
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Attachment>().ToTable("Attachments");
            modelBuilder.Entity<CourseBan>().ToTable("CourseBans");

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.SpecialCode)
                .IsUnique();

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

            modelBuilder.Entity<CourseMember>()
                .HasKey(cm => new { cm.UserId, cm.CourseId });

            modelBuilder.Entity<CourseMember>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.EnrolledIn)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseMember>()
                .HasOne(ut => ut.Course)
                .WithMany(c => c.Members)
                .HasForeignKey(ut => ut.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Course)
                .WithMany(c => c.Tasks)
                .HasForeignKey(pt => pt.CourseId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Author)
                .WithMany()
                .HasForeignKey(pt => pt.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subtask>()
                .HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatorId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Subtask>()
                .HasOne(s => s.LastStatusUpdater)
                .WithMany()
                .HasForeignKey(s => s.LastStatusUpdaterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subtask>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subtasks)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Comment)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseBan>()
                .HasOne(cb => cb.Course)
                .WithMany()
                .HasForeignKey(cb => cb.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseBan>()
                .HasOne(cb => cb.User)
                .WithMany()
                .HasForeignKey(cb => cb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(pt => pt.TaskAssignments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Team)
                .WithMany(t => t.TaskAssignments)
                .HasForeignKey(ta => ta.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignment>()
                .HasMany(ta => ta.Subtasks)
                .WithOne(s => s.TaskAssignment)
                .HasForeignKey(s => s.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<TaskAssignment>()
                .HasMany(ta => ta.Comments)
                .WithOne(c => c.TaskAssignment)
                .HasForeignKey(c => c.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<TaskAssignment>()
                .HasMany(ta => ta.TurnInAttachments)
                .WithOne(a => a.TaskAssignment)
                .HasForeignKey(a => a.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTeam>()
                .HasOne(ut => ut.Team)
                .WithMany(t => t.UserTeams)
                .HasForeignKey(ut => ut.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}