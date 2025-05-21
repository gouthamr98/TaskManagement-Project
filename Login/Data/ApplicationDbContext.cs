using Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Login.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {

        }
        public DbSet<Users> User{ get; set; }
        public DbSet<Roles> Role{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //seed roles
            modelBuilder.Entity<Roles>().HasData(
                new Roles {RoleId=1,RoleName="Admin" },
                new Roles {RoleId=2,RoleName="User" }
                );

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role).WithMany(r => r.User).HasForeignKey(u => u.RoleID);


        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await User.AnyAsync(u => u.Email == email); // Using the DbSet<User> to check if the user exists
        }

    }
}
