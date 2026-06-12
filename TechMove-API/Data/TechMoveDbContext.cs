using Microsoft.EntityFrameworkCore;
using TechMove_API.Models;


namespace TechMove_API.Data
{
    public class TechMoveDbContext : DbContext
    {
        public TechMoveDbContext(DbContextOptions<TechMoveDbContext> options) : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        public  DbSet<User> Users { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Store ContractStatus as string instead of int
            modelBuilder.Entity<Contract>()
                .Property(c => c.Status)
                .HasConversion<string>();
            // Store ServiceRequestStatus as string instead of int

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ServiceRequest>().Property(st => st.Type).HasConversion<string>();
            // preconfigured users with bcrypt  hashed passwords
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "John Kalimali",
                    Email = "johnkali27@techmove.com",
                    Phone = "0671239012",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@212")
                },
                new User
                {
                    Id = 2,
                    Name = "Thato Rathana",
                    Email = "thatorathana@techmove.com",
                    Phone = "0787129900",
                    Password = BCrypt.Net.BCrypt.HashPassword("thatoR@13")
                },
                new User
                {
                    Id = 3,
                    Name = "Emanuel Richard",
                    Email = "emanuelrichard30@techmove.com",
                    Phone = "0718901458",
                    Password = BCrypt.Net.BCrypt.HashPassword("EmanuRich12@12")
                }

            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
