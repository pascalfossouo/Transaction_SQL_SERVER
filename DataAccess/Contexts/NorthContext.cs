using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ObjectsAffaire.Models;

namespace DataAccess.Contexts
{
    public class NorthContext:DbContext
    {
        public NorthContext(DbContextOptions options):base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
    }
}
