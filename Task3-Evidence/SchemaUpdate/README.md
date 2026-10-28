# Task 3 — C.3 Schema Update Evidence

## Schema change

Added `DeliveryStatus` to the `TaxCertificates` entity.

- Data type: string
- Maximum length: 20 characters
- Default: `Pending`
- Purpose: distinguishes delivery tracking from certificate generation status.

## Demo database

The Render Demo environment uses SQLite. On startup, the app checks
`PRAGMA table_info('TaxCertificates')` and adds the column only if it
does not already exist.

This avoids failing when a Demo database was created using an older schema.

## Azure SQL

`001_Add_TaxCertificate_DeliveryStatus_AzureSql.sql` contains the
equivalent idempotent SQL Server schema update.

**The Azure SQL script has not been executed.** The Azure subscription
was disabled and reactivation failed. The alternative SQLite update
must not be described as an Azure SQL update.

## Evidence to capture

1. Successful solution build and test output.
2. Render Demo logs showing the schema check/update.
3. The SQL script retained as the prepared Azure SQL change.
4. The lecturer/campus communication and Azure subscription error
   screenshots documenting the limitation.
