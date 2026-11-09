using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Models
{
    public partial class HotelManagementContext
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;" +
            "Database=HotelManagement;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        public HotelManagementContext()
        {
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }
    }
}