using DB2BM.Extensions.PgSql.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DB2BM.Extensions.PgSql
{
    public class PostgreDbContext : DbContext
    {
        public DbSet<PostgreTable> Tables { get; set; }

        public DbSet<PostgreFunction> Functions { get; set; }

        public DbSet<PostgreField> Fields { get; set; }

        public DbSet<PostgreParams> Params { get; set; }

        public DbSet<PostgreRelation> Relations { get; set; }

        public DbSet<PostgreUserDefinedType> UDTs { get; set; }

        public DbSet<PostgreUDTField> UDTFs { get; set; }

        public DbSet<PostgreUDEnumOption> EnumsOptions { get; set; }

        public DbSet<PostgreSequence> Sequences { get; set; }

        public DbSet<PostgreFunctionResult> FuntionResults { get; set; }

        public string ConnectionString { get; }

        public PostgreDbContext(DbContextOptions options, string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostgreUDEnumOption>(config => config.HasNoKey());

            modelBuilder.HasDefaultSchema("information_schema");

            modelBuilder.Entity<PostgreTable>(config =>
            {
                config.HasKey(t => t.Name);
                config.Property(t => t.Name).HasColumnName("table_name");
                config.Property(t => t.SchemaName).HasColumnName("table_schema");
                config.HasMany(t => t.Fields).WithOne(f => f.Table);
                config.ToTable("tables");
            });

            modelBuilder.Entity<PostgreSequence>(config =>
            {
                config.HasKey(s => s.Name);
                config.Property(s => s.Name).HasColumnName("sequence_name");
                config.Property(s => s.Increment).HasColumnName("increment");
                config.Property(s => s.Start).HasColumnName("start_value");
                config.Property(s => s.MinValue).HasColumnName("minimum_value");
                config.Property(s => s.MaxValue).HasColumnName("maximum_value");
                config.ToTable("sequences");
            });

            modelBuilder.Entity<PostgreField>(config =>
            {
                config.HasKey(f => new { f.Name, f.TableName });
                config.Property(f => f.Name).HasColumnName("column_name");
                config.HasOne(f => f.Table).WithMany(t => t.Fields);
                config.Property(f => f.TableName).HasColumnName("table_name");
                config.Property(f => f.OrdinalPosition).HasColumnName("ordinal_position");
                config.Property(f => f.IsNullable).HasColumnName("is_nullable");
                config.Property(f => f.TypeName).HasColumnName("udt_name");
                config.Property(f => f.CharacterMaximumLength).HasColumnName("character_maximum_length");
                config.Property(f => f.Default).HasColumnName("column_default");
                config.ToTable("columns");
            });

            modelBuilder.Entity<PostgreFunction>(config =>
            {
                config.HasKey(f => f.SpecificName);
                config.Property(f => f.Name).HasColumnName("routine_name");
                config.HasMany(f => f.Params).WithOne(p => p.Function);
                config.Property(f => f.SpecificName).HasColumnName("specific_name");
                config.Property(f => f.SpecificSchema).HasColumnName("routine_schema");
                config.Property(f => f.FunctionType).HasColumnName("routine_type");
                config.Property(f => f.ReturnClause)
                    .HasColumnName("return_clause")
                    .HasComputedColumnSql("PG_GET_FUNCTION_RESULT(CAST(REPLACE(specific_name, CONCAT(routine_name, '_'), '') AS INT))");
                config.Property(f => f.ReturnType).HasColumnName("type_udt_name");
                config.Property(f => f.Definition).HasColumnName("routine_definition");
                config.Property(f => f.LanguageDefinition).HasColumnName("external_language");
                config.ToTable("routines");

            });

            modelBuilder.Entity<PostgreParams>(config =>
            {
                config.HasKey(p => new { p.OrdinalPosition, p.FunctionSpecificName });
                config.Property(p => p.FunctionSpecificName).HasColumnName("specific_name");
                config.Property(p => p.OrdinalPosition).HasColumnName("ordinal_position");
                config.HasOne(p => p.Function).WithMany(f => f.Params);
                config.Property(p => p.ParameterMode).HasColumnName("parameter_mode");
                config.Property(p => p.IsResult).HasColumnName("is_result");
                config.Property(p => p.Name).HasColumnName("parameter_name");
                config.Property(p => p.TypeName).HasColumnName("udt_name");
                config.ToTable("parameters");
            });

            modelBuilder.Entity<PostgreFunctionResult>(config => {
                config.HasNoKey();
                config.Property(p => p.Clause).HasColumnName("clause");
            });

            modelBuilder.Entity<PostgreRelation>(config =>
            {
                config.HasKey(r => r.ConstraintName);
                config.Property(r => r.ConstraintName).HasColumnName("constraint_name");
                config.Property(r => r.SchemaName).HasColumnName("constraint_schema");
                config.Property(r => r.TableName).HasColumnName("table_name");
                config.Property(r => r.ConstraintType).HasColumnName("constraint_type");
                config.HasOne(r => r.RelationColumn).WithOne(x => x.Relation).HasForeignKey<PostgreRelationColumnUsage>(y => y.ConstraintName);
                config.HasOne(r => r.KeyColumn).WithOne(x => x.Relation).HasForeignKey<PostgreKeyColumnUsage>(y => y.ConstraintName);
                config.ToTable("table_constraints");
            });

            modelBuilder.Entity<PostgreRelationColumnUsage>(config =>
            {
                config.HasKey(r => r.ConstraintName);
                config.Property(r => r.ConstraintName).HasColumnName("constraint_name");
                config.Property(r => r.ColumnName).HasColumnName("column_name");
                config.Property(r => r.TableName).HasColumnName("table_name");
                config.HasOne(r => r.Relation).WithOne(x => x.RelationColumn);
                config.ToTable("constraint_column_usage");
            });

            modelBuilder.Entity<PostgreKeyColumnUsage>(config =>
            {
                config.HasKey(k => k.ConstraintName);
                config.Property(k => k.ConstraintName).HasColumnName("constraint_name");
                config.Property(k => k.ColumnName).HasColumnName("column_name");
                config.Property(k => k.TableName).HasColumnName("table_name");
                config.HasOne(k => k.Relation).WithOne(x => x.KeyColumn);
                config.ToTable("key_column_usage");
            });

            modelBuilder.Entity<PostgreUserDefinedType>(config =>
            {
                config.HasKey(udt => udt.Name);
                config.Property(udt => udt.Name).HasColumnName("user_defined_type_name");
                config.Property(udt => udt.Schema).HasColumnName("user_defined_type_schema");
                config.Property(udt => udt.Category).HasColumnName("user_defined_type_category");
                config.HasMany(udt => udt.Fields).WithOne(udtf => udtf.UDT);
                config.ToTable("user_defined_types");
            });

            modelBuilder.Entity<PostgreUDTField>(config =>
            {
                config.HasKey(udtf => new { udtf.Name, udtf.UDTName });
                config.Property(udtf => udtf.Name).HasColumnName("attribute_name");
                config.Property(udtf => udtf.UDTName).HasColumnName("udt_name");
                config.Property(udtf => udtf.OrdinalPosition).HasColumnName("ordinal_position");
                config.Property(udtf => udtf.Default).HasColumnName("attribute_default");
                config.Property(udtf => udtf.CharacterMaximumLength).HasColumnName("character_maximum_length");
                config.Property(udtf => udtf.IsNullable).HasColumnName("is_nullable");
                config.Property(udtf => udtf.TypeName).HasColumnName("attribute_udt_name");
                config.HasOne(udtf => udtf.UDT).WithMany(udt => udt.Fields);
                config.ToTable("attributes");
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        public static DbContextOptions GetOptions(string connectionString)
        {
            var optionBuilder = new DbContextOptionsBuilder();
            return optionBuilder.UseNpgsql(connectionString).Options;
        }
    }
}
