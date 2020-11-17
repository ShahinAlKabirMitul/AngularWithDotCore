using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SocialApp.Entities;

namespace SocialApp.Data
{
  public  class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }
        public DbSet<AppUser> Users { set; get; }
    }
}
