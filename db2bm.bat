@echo off
dotnet run --project db2bm -- --h vm-win2012-srv1 --db Test --u develop --p develop --orm efcore --dbms postgre --splist db2bm\sp_list.json %*