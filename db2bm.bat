@echo off
dotnet run --project db2bm -- --h pgsql.postgres.database.azure.com --db Test --u develop@pgsql --p develop --orm efcore --dbms postgre --splist db2bm\sp_list.json %*