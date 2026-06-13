# DBML Generator Plugin
[![Auto build](https://github.com/DKorablin/Plugin.DbmlGenerator/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.DbmlGenerator/releases/latest)

A [SAL](https://github.com/DKorablin/SAL.Windows) plugin that generates **LINQ to SQL (`.dbml`)** mapping files from Microsoft SQL Server stored procedures and arbitrary SQL queries.

## Features

- **DBML generation** — introspects result-set columns of stored procedures or any SQL statement and produces ready-to-use `<ElementType>` / `<Column>` DBML XML fragments.
- **Stored procedure parameters** — automatically maps input/output parameters to `<Function>` / `<Parameter>` entries.
- **Grid view** — executes any SQL command and displays the result set in a sortable grid.
- **XML export** — serializes the result `DataSet` to XML.
- **Template formatting** — applies a custom `String.Format`-style template to each result row for code-generation or text-transformation scenarios.
- **Multiple connections** — stores and manages any number of named database connections (name, connection string, ADO.NET provider).
- **Comment embedding** — optionally writes the source SQL command as an XML comment inside the generated DBML.
- **XSD companion** — optionally saves an XSD schema alongside the DataSet XML output.

## Requirements

| Component | Version |
|-----------|---------|
| .NET Framework | 4.8 |
| .NET | 8.0 (Windows) |
| SAL.Windows | see `Plugin.DbmlGenerator.csproj` |
| ADO.NET provider | Any provider registered via `DbProviderFactories` (e.g. `System.Data.SqlClient`) |

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.DbmlGenerator should appear in the plugin list (Data → ORM → DBML Generator).

## Usage

1. Open **Data → ORM → DBML Generator** to show the generator panel.
2. In the plugin **Options** page, add one or more database connections (name, connection string, ADO.NET provider invariant name).
3. In the generator panel:
   - Select a connection from the drop-down.
   - Optionally select a stored procedure from the procedure list to include parameter mappings.
   - Enter a SQL command (e.g. `EXEC dbo.MyProcedure @param1 = 1`) in the SQL text box.
   - Click the **Command** button and choose an action:
     | Action | Description |
     |--------|-------------|
     | **DBML** | Generates DBML XML for the result-set columns and procedure parameters. |
     | **Grid** | Displays the result set in a data grid. |
     | **XML** | Exports the result set as DataSet XML. |
     | **Template** | Formats each result row using the configured SQL Template. |

## Configuration

Plugin settings are persisted by the SAL host and exposed in the options panel:

| Setting | Default | Description |
|---------|---------|-------------|
| Save with XSD | `false` | Save DataSet XML together with an XSD schema file. |
| Add comments | `true` | Embed the source SQL command as an XML comment in the DBML output. |
| SQL Template | _(empty)_ | A `String.Format` template applied to each row when using the **Template** action. |