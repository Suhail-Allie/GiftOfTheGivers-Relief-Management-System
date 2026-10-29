# Part 3 - Continuous Deployment Evidence

Date: 29 October 2026
Repository branch: main

## CI configuration

The Gift of the Givers CI workflow runs on GitHub Actions.
Commit f29c582 was associated with a successful CI run.

## Render deployment configuration

Both existing Render services have been configured to use
After CI Checks Pass:

- Web service: giftofthegivers-relief-management-system
- Function service: gotg-tax-certificate-demo

The next commit after enabling these settings is intended to
verify automatic deployment after CI succeeds.

## Live demonstration endpoints

Web application:
https://giftofthegivers-relief-management-system.onrender.com

Tax-certificate demo Function:
https://gotg-tax-certificate-demo.onrender.com/api/GenerateTaxCertificate

## SQL Server schema migration workaround

The SQL Server Schema Deployment Workaround completed successfully
for commit f29c582. Its artifact is named
sql-server-schema-deployment-evidence.

The migration added dbo.TaxCertificates.DeliveryStatus as
nvarchar, NOT NULL, with default (N'Pending'). The default-value
test passed using a temporary SQL Server 2022 database.

Limitation: this is not a persistent Azure SQL Database deployment.

## Evidence still to capture

- GitHub Actions run for the deployment-evidence commit.
- Render web service deployment history for that commit.
- Render Function service deployment history for that commit.
- Live endpoint checks after deployment.
- Screenshots showing successful deployment completion.