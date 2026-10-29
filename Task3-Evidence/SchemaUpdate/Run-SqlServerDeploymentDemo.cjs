'use strict';

const fs = require('node:fs');
const path = require('node:path');
const sql = require('mssql');

const password = process.env.SQL_SERVER_PASSWORD;
if (!password) {
  console.error('SQL_SERVER_PASSWORD environment variable is required.');
  process.exit(1);
}

const databaseName = 'GOTGSchemaDeploymentDemo';
const migrationPath = path.join(
  process.cwd(),
  'Task3-Evidence',
  'SchemaUpdate',
  '001_Add_TaxCertificate_DeliveryStatus_AzureSql.sql'
);

const baseConfig = {
  user: 'sa',
  password,
  server: '127.0.0.1',
  port: 1433,
  options: {
    encrypt: false,
    trustServerCertificate: true
  },
  connectionTimeout: 5000,
  requestTimeout: 15000,
  pool: {
    max: 2,
    min: 0,
    idleTimeoutMillis: 30000
  }
};

const sleep = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));

async function connectWithRetry(database) {
  let lastError;

  for (let attempt = 1; attempt <= 45; attempt += 1) {
    const pool = new sql.ConnectionPool({ ...baseConfig, database });
    try {
      await pool.connect();
      console.log(`Connected to SQL Server database '${database}'.`);
      return pool;
    } catch (error) {
      lastError = error;
      await pool.close().catch(() => {});
      console.log(`SQL Server readiness check ${attempt}/45; retrying in 2 seconds.`);
      await sleep(2000);
    }
  }

  throw new Error(`SQL Server was not ready in time: ${lastError?.message ?? 'unknown error'}`);
}

async function main() {
  console.log('Part 3 C.3 workaround: automated T-SQL schema migration demonstration.');
  console.log('Target: disposable SQL Server 2022 service within a GitHub-hosted runner.');
  console.log('This is not a persistent Azure SQL Database deployment.');
  console.log(`Migration source: ${migrationPath}`);

  if (!fs.existsSync(migrationPath)) {
    throw new Error(`Migration file was not found: ${migrationPath}`);
  }

  let masterPool;
  let databasePool;

  try {
    masterPool = await connectWithRetry('master');

    const lookup = await masterPool.request().query(
      `SELECT DB_ID(N'${databaseName}') AS DatabaseId;`
    );

    if (lookup.recordset[0].DatabaseId == null) {
      // Run CREATE DATABASE in its own batch, as required by SQL Server.
      await masterPool.request().query(`CREATE DATABASE [${databaseName}];`);
      console.log(`Created disposable database '${databaseName}'.`);
    } else {
      console.log(`Database '${databaseName}' already exists for this run.`);
    }

    await masterPool.close();
    masterPool = null;

    databasePool = await connectWithRetry(databaseName);

    // Represent the pre-update table. The migration under test adds only DeliveryStatus.
    await databasePool.request().query(`
      IF OBJECT_ID(N'dbo.TaxCertificates', N'U') IS NULL
      BEGIN
        CREATE TABLE [dbo].[TaxCertificates]
        (
          [TaxCertificateID] INT IDENTITY(1,1) NOT NULL
            CONSTRAINT [PK_TaxCertificates] PRIMARY KEY,
          [DonationID] INT NOT NULL,
          [CertificateNumber] NVARCHAR(100) NOT NULL,
          [GeneratedAt] DATETIME2 NOT NULL
            CONSTRAINT [DF_TaxCertificates_GeneratedAt]
            DEFAULT (SYSUTCDATETIME()),
          [FileReference] NVARCHAR(500) NULL,
          [CertificateStatus] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_TaxCertificates_CertificateStatus]
            DEFAULT (N'Generated')
        );
      END;
    `);
    console.log('Baseline dbo.TaxCertificates table prepared.');

    const migration = fs.readFileSync(migrationPath, 'utf8');
    // SQLCMD's GO separator is a batch separator, not a T-SQL statement.
    const batches = migration
      .split(/^[\t ]*GO[\t ]*$/gim)
      .map(batch => batch.trim())
      .filter(Boolean);

    if (batches.length === 0) {
      throw new Error('The schema migration file did not contain any executable batches.');
    }

    for (let index = 0; index < batches.length; index += 1) {
      await databasePool.request().batch(batches[index]);
      console.log(`Applied migration batch ${index + 1}/${batches.length}.`);
    }

    const schemaResult = await databasePool.request().query(`
      SELECT
        c.[name] AS [ColumnName],
        TYPE_NAME(c.[user_type_id]) AS [DataType],
        c.[is_nullable] AS [IsNullable],
        dc.[definition] AS [DefaultDefinition]
      FROM sys.columns AS c
      LEFT JOIN sys.default_constraints AS dc
        ON dc.[parent_object_id] = c.[object_id]
       AND dc.[parent_column_id] = c.[column_id]
      WHERE c.[object_id] = OBJECT_ID(N'dbo.TaxCertificates')
        AND c.[name] = N'DeliveryStatus';
    `);

    const column = schemaResult.recordset[0];
    if (!column) {
      throw new Error('Verification failed: DeliveryStatus was not created.');
    }
    if (String(column.DataType).toLowerCase() !== 'nvarchar') {
      throw new Error(`Verification failed: unexpected type '${column.DataType}'.`);
    }
    if (Number(column.IsNullable) !== 0) {
      throw new Error('Verification failed: DeliveryStatus must be NOT NULL.');
    }
    if (!String(column.DefaultDefinition ?? '').includes('Pending')) {
      throw new Error(`Verification failed: unexpected default '${column.DefaultDefinition}'.`);
    }

    console.log('Verified schema metadata:');
    console.log(`  Column: ${column.ColumnName}`);
    console.log(`  SQL type: ${column.DataType}`);
    console.log(`  Nullable: ${Boolean(column.IsNullable)}`);
    console.log(`  Default: ${column.DefaultDefinition}`);

    const certificateNumber = `CI-SCHEMA-${Date.now()}`;
    const defaultResult = await databasePool.request()
      .input('certificateNumber', sql.NVarChar(100), certificateNumber)
      .query(`
        INSERT INTO dbo.TaxCertificates ([DonationID], [CertificateNumber])
        VALUES (99999, @certificateNumber);

        SELECT [DeliveryStatus]
        FROM dbo.TaxCertificates
        WHERE [CertificateNumber] = @certificateNumber;
      `);

    const actualDefault = defaultResult.recordset[0]?.DeliveryStatus;
    if (actualDefault !== 'Pending') {
      throw new Error(`Default-value check failed: expected 'Pending', got '${actualDefault}'.`);
    }

    console.log(`Default-value check: passed (new row received '${actualDefault}').`);
    console.log('SUCCESS: T-SQL migration applied and verified in the disposable SQL Server service.');
    console.log('LIMITATION: This workflow does not persist data and is not an Azure SQL deployment.');
  } finally {
    if (databasePool) await databasePool.close().catch(() => {});
    if (masterPool) await masterPool.close().catch(() => {});
  }
}

main().catch(error => {
  console.error('SQL Server schema deployment demonstration FAILED.');
  console.error(error?.stack ?? error);
  process.exitCode = 1;
});
