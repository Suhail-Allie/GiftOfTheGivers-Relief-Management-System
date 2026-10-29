# C.3 SQL schema deployment workaround evidence

## One-time setup

In the GitHub repository, open **Settings → Secrets and variables → Actions → New repository secret**. Create `CI_SQLSERVER_SA_PASSWORD` with a unique, strong password for this disposable SQL Server service. Do not put the password in source control or reuse any production or personal password.

## What the automated demonstration does

The `SQL Server Schema Deployment Workaround` GitHub Actions workflow runs after the `Gift of the Givers CI` workflow completes successfully on `main`. It starts a disposable SQL Server 2022 service on the GitHub-hosted runner, prepares a minimal pre-update `dbo.TaxCertificates` table, executes the repository's T-SQL migration file, and checks that `DeliveryStatus` is `NVARCHAR`, `NOT NULL`, and defaults to `Pending`. It also inserts one temporary record to verify the default value is applied.

The workflow uploads a `sql-server-schema-deployment-evidence` artifact containing the migration and verification log. This supplies evidence of an automated T-SQL schema migration against SQL Server and avoids requiring Docker or SQL Server on the student's computer.

## Limitation to disclose

This is a workaround, not a completed Azure SQL deployment. The database exists only for the duration of the GitHub Actions run and is discarded afterward. The Azure SQL script has not been run against the original Azure SQL database because the Azure subscription is disabled. Do not describe this as a persistent cloud database update or as successful Azure SQL deployment.

## Evidence to retain

1. Screenshot of the successful `SQL Server Schema Deployment Workaround` workflow run, showing the commit and green status.
2. Downloaded `sql-server-schema-deployment-evidence` artifact from that workflow run.
3. Screenshot or copy of the log showing the column metadata and `Default-value check: passed`.
4. The existing Azure subscription-disabled and lecturer/campus-support evidence explaining why Azure SQL could not be used.
