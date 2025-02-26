using its_bot.Models;
using Microsoft.EntityFrameworkCore;

namespace its_bot.Data
{
    public class SupportJiraContext : DbContext
    {
        public DbSet<AuthorizationJira> AuthorizationJiras { get; set; }

        public SupportJiraContext(DbContextOptions<SupportJiraContext> options) : base(options)
        { }
    }
}
