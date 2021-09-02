
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IotDash.Data.Model;

namespace IotDash.Data {

    public class DataContext : IdentityDbContext {

        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        public DbSet<Device> Devices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }

}
