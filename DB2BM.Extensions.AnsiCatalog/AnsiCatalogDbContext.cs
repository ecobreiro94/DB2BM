using DB2BM.Extensions.AnsiCatalog.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.AnsiCatalog
{
    public class AnsiCatalogDbContext : DbContext
    {
        public DbSet<AnsiTable> Tables { get; set; }

        public DbSet<AnsiField> Fields { get; set; }

        public DbSet<AnsiRoutine> Routines { get; set; }

        public DbSet<AnsiParams> Params { get; set; }

        public DbSet<AnsiRelationship> Relationships { get; set; }

        public DbSet<AnsiSequence> Sequences { get; set; }

        public string ConnectionString { get; set; }

    }
}
