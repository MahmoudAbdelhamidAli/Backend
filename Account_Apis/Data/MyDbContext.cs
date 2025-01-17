using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Account_Apis.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Account_Apis.Data
{
    public class MyDbContext : IdentityDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }
        // Add DbSet for User
        public DbSet<User> Users { get; set; }

        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // one-to-many relationship (one user can have many courses) ,but no cascading delete
            modelBuilder.Entity<Course>()
                .HasOne(c => c.User)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
                

            // one-to-many relationship (one course can have many users) ,but no cascading delete
            modelBuilder.Entity<User>()
                .HasOne(u => u.Course)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
                
        }



        
        
    }
}