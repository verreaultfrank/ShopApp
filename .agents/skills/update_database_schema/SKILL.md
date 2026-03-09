---
name: update_database_schema
description: Make sure that each time a C# Model is updated, the database tables associated with it are also updated to reflect the new or modified fields.
---

# Update Database Schema Instructions

When you are asked to update a C# Model (for example, in `JobHunter.Models` or `JobHunterDashboard.Models`), and this model represents a database entity, you **MUST** also ensure the corresponding database table schema is updated.

## Steps to follow:

1. **Identify the affected Model and Properties:**
   - Determine which C# class was modified (e.g., `JobOpportunity`, `DashboardJobOpportunity`).
   - Identify the specific properties that were added, removed, or changed.
   - Note the data types of these properties (e.g., `string`, `int`, `decimal?`, `DateTime`, `Enum`).

2. **Map C# Types to SQL Types:**
   - `string` -> `NVARCHAR(MAX)` or a specific length.
   - `int` -> `INT`.
   - `decimal` -> `DECIMAL(18,2)` or similar.
   - `DateTime` -> `DATETIME2`.
   - `bool` -> `BIT`.
   - `Enum` / Arrays of Enums -> Often stored as `NVARCHAR` (comma-separated strings) or `INT` depending on the existing architecture. (Check how the project maps these in the repository classes first!).

3. **Locate the Database Interactions:**
   - Find the Repository classes (e.g., `JobLeadRepository.cs`) to see which table corresponds to the model (e.g., `JobLeads`).
   - Look at the SQL `INSERT`, `UPDATE`, and `SELECT` statements in the repository to see if they need to be updated to include the new fields.

4. **Update the Database Schema:**
   - Since this project uses raw SQL and Dapper (via `SqlConnection` / `db.ExecuteAsync`), there are no EF Core migrations.
   - You must execute an `ALTER TABLE` SQL command to update the schema in the development environment.
   - You can use the `mcp_sql-server_executeSqlBatch` or `mcp_sql-server_alterTable` MCP tool to directly update the database if the SQL server is connected.
   - Example raw SQL to add a column:
     ```sql
     ALTER TABLE [TableName] ADD [NewColumnName] [SqlDataType] NULL;
     ```

5. **Update SQL Queries in Code:**
   - Ensure you update any `INSERT` or `UPDATE` statements in the corresponding repository (like `JobLeadRepository.cs`) to read and write the new fields to the database.

6. **Verify:**
   - After applying the changes, ensure the repository logic is correct and the database table actually has the newly added columns.
