using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace its_bot
{
    public class SupportJiraContext: DbContext
    {
        public DbSet<AuthorizationJira> AuthorizationJiras { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"{InfoManager.getServerInfo()}");
        }
    }
}
