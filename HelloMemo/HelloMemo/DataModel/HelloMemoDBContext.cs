using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace HelloMemo.DataModel
{
    class HelloMemoDBContext : DbContext
    {
        public HelloMemoDBContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string fName = Path.Combine(GlobalVars.PathApp, GlobalVars.LocalDbFileName);
            optionsBuilder.UseSqlite($"Filename={fName}");
        }

        public DbSet<Word> Words { get; set; }
        public DbSet<Sample> Samples { get; set; }
    }
}
