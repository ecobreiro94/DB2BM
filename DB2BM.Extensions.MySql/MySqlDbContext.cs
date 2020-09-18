using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DB2BM.Extensions.AnsiCatalog;
using DB2BM.Extensions.AnsiCatalog.Entities;
using DB2BM.Extensions.MySql.Entities;

namespace DB2BM.Extensions.MySql
{
    public class MySqlDbContext : AnsiCatalogDbContext
    {

        public new DbSet<MySqlRelationship> Relationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("information_schema");
            
            modelBuilder.Entity<AnsiTable>(config =>
            {
                config.HasKey(t => t.Name);
                config.Property(t => t.Name).HasColumnName("table_name");
                config.Property(t => t.SchemaName).HasColumnName("table_schema");
                config.HasMany(t => t.Fields).WithOne(f => f.Table);
                config.ToTable("tables");
            });

            modelBuilder.Ignore<AnsiSequence>();

            modelBuilder.Entity<AnsiField>(config =>
            {
                config.HasKey(f => new { f.Name, f.TableName });
                config.Property(f => f.Name).HasColumnName("column_name");
                config.HasOne(f => f.Table).WithMany(t => t.Fields);
                config.Property(f => f.TableName).HasColumnName("table_name");
                config.Property(f => f.OrdinalPosition).HasColumnName("ordinal_position");
                config.Property(f => f.IsNullable).HasColumnName("is_nullable");
                config.Property(f => f.DataTypeName).HasColumnName("data_type");
                config.Ignore(f => f.UdtName);
                config.Property(f => f.CharacterMaximumLength).HasColumnName("character_maximum_length");
                config.Property(f => f.Default).HasColumnName("column_default");
                config.ToTable("columns");
            });

            modelBuilder.Entity<AnsiRoutine>(config =>
            {
                config.HasKey(f => f.SpecificName);
                config.Property(f => f.Name).HasColumnName("routine_name");
                config.HasMany(f => f.Params).WithOne(p => p.Function);
                config.Property(f => f.SpecificName).HasColumnName("specific_name");
                config.Property(f => f.SpecificSchema).HasColumnName("routine_schema");
                config.Property(f => f.RoutineType).HasColumnName("routine_type");
                config.Ignore(f => f.ReturnClause);
                config.Ignore(f => f.MaxDynamicResultSets);
                config.Property(f => f.ReturnDataType).HasColumnName("data_type");
                config.Ignore(f => f.ReturnUdtType);
                config.Property(f => f.Definition).HasColumnName("routine_definition");
                config.Property(f => f.RoutineLanguage).HasColumnName("routine_body");
                config.Property(f => f.ExternalLanguage).HasColumnName("external_language");
                config.ToTable("routines");

            });

            modelBuilder.Entity<AnsiParams>(config =>
            {
                config.HasKey(p => new { p.OrdinalPosition, p.FunctionSpecificName });
                config.Property(p => p.FunctionSpecificName).HasColumnName("specific_name");
                config.Property(p => p.OrdinalPosition).HasColumnName("ordinal_position");
                config.HasOne(p => p.Function).WithMany(f => f.Params);
                config.Property(p => p.ParameterMode).HasColumnName("parameter_mode");
                config.Ignore(p => p.IsResult);
                config.Property(p => p.Name).HasColumnName("parameter_name");
                config.Property(p => p.DataTypeName).HasColumnName("data_type");
                config.Ignore(p => p.UdtName);
                config.ToTable("parameters");
            });

            modelBuilder.Ignore<AnsiRelationship>();

            modelBuilder.Entity<MySqlRelationship>(config => 
            {
                config.HasKey(r => new { r.TableName, r.ConstraintName });
                config.Property(r => r.ConstraintName).HasColumnName("constraint_name");
                config.Property(r => r.SchemaName).HasColumnName("constraint_schema");
                config.Property(r => r.TableName).HasColumnName("table_name");
                config.Property(r => r.ConstraintType).HasColumnName("constraint_type");
                config.Ignore(r => r.RelationColumn);
                config.HasOne(r => r.KeyColumn).WithOne().HasForeignKey<MySqlKeyColumnUsage>(x => new { x.TableName, x.ConstraintName });
                config.ToTable("table_constraints");
            });

            modelBuilder.Ignore<AnsiRelationColumnUsage>();
            modelBuilder.Ignore<AnsiKeyColumnUsage>();

            modelBuilder.Entity<MySqlKeyColumnUsage>(config =>
            {
                config.HasKey(k => new { k.TableName, k.ConstraintName });
                config.Property(k => k.ConstraintName).HasColumnName("constraint_name");
                config.Property(k => k.ColumnName).HasColumnName("column_name");
                config.Property(k => k.TableName).HasColumnName("table_name");
                config.Property(k => k.ReferencedColumnName).HasColumnName("referenced_column_name");
                config.Property(k => k.ReferencedTableName).HasColumnName("referenced_table_name");
                config.ToTable("key_column_usage");
            });

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectionString);
        }
    }
}
