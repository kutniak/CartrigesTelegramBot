using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CartrigesTelegramBot
{
    public class MyDbContext : DbContext
    {
        public MyDbContext()
               : base("MyDbContextString")
        { }

        public DbSet<Cartrige> Cartridges { get; set; }
    }
}
