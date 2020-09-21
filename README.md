# DB2BM
General tool to migrate an application backend based in stored procedures to a business model based in an OR/Mapping

There is a huge amount of information in DB, which can be accessed via Stored Procedure. To continue using this data and the legacy code in new applications developed with the modern technologies and architectures was developed a multiplatform tool to automate the migration process, of DB stored procedures code to objects and services model using an ORM.

The tool has a three-layer architecture, based on plugins, which consists of: 
 1. an execution layer, where the migration process takes place; 
 2. an abstraction layer, where the interfaces that represent the process actors are defined; and 
 3. an Extensions Layer, in which the actors for certain DBMS and ORMs are implemented. 

As first extensions, were implemented those which are associated to PostgreSQL DB and code generation using EF Core.

The extraction of catalog information and the code transpilation were necessary to carry out migration process. Some libraries like EFCore, ANTLR and StringTemplate were used for this purpose. 

Resulting AST from Syntactic Analysis can be traversed with a Visitor Patter in order to perform Semantic Analysis and Code Generation.
An extensible tool, that contributes to increase productivity in applications development, was obtained, even when there are complex situations that have not been supported in this first version.

