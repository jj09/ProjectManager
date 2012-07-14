using System.Data.Entity;
using System.Data.Entity.Database;

namespace ProjectManager.Models
{
    public class ProjectManagerEntities : DbContext
    {
        public DbSet<Course>        Courses  { get; set; }
        public DbSet<Enrollment>    Enrollments  { get; set; }
        public DbSet<Project>       Projects { get; set; }
        public DbSet<ProjectData>   ProjectsData { get; set; }
        public DbSet<News>          News { get; set; }
        public DbSet<User>          Users { get; set; }

        //protected override void OnModelCreating(System.Data.Entity.ModelConfiguration.ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
        //}
    }
}