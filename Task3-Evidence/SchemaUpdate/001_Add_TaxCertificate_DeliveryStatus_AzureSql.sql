/*
    Task 3 - C.3
    Adds delivery tracking to TaxCertificates.

    PREPARED SCRIPT — NOT EXECUTED AGAINST AZURE SQL.
    The Azure subscription is disabled. Execute only after
    authorised access to the intended database is restored.
*/

IF COL_LENGTH(N'dbo.TaxCertificates', N'DeliveryStatus') IS NULL
BEGIN
    ALTER TABLE [dbo].[TaxCertificates]
    ADD [DeliveryStatus] NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_TaxCertificates_DeliveryStatus]
        DEFAULT (N'Pending') WITH VALUES;
END;
GO
