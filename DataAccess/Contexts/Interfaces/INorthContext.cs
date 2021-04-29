using Microsoft.EntityFrameworkCore;
using ObjectsAffaire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Contexts.Interfaces
{
    public interface INorthContext
    {
        DbSet<Employee> Employees { get; set; }
    }
}
