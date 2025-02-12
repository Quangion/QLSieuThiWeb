using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Data
{
    public class QLSieuThiWebContext : DbContext
    {
        public QLSieuThiWebContext (DbContextOptions<QLSieuThiWebContext> options)
            : base(options)
        {
        }

        public DbSet<QLSieuThiWeb.Models.TKMK> TKMK { get; set; } = default!;
    }
}
