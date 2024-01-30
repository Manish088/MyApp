
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.DataAccessLayer
{
    public class ApplicatonDbContext:IdentityDbContext
    {
        public ApplicatonDbContext(DbContextOptions<ApplicatonDbContext> options):base(options)
        {

        }
        public DbSet<Category> categories { get; set; }

        public DbSet<Product> products { get; set; }

        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<Cart> Carts{ get; set; }

        public DbSet<OrderHeader> orderheaders { get; set; }

        public DbSet<OrderDetail> orderdetails { get;set; }
    }
}
