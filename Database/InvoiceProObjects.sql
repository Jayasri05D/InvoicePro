
USE InvoiceProDB;
GO

/* =====================================================
   1. DATABASE VIEW: Invoice summary
   ===================================================== */

CREATE OR ALTER VIEW dbo.vw_InvoiceSummary
AS
SELECT
    i.InvoiceId,
    i.InvoiceNumber,
    c.Name AS CustomerName,
    i.InvoiceDate,
    i.DueDate,
    i.Subtotal,
    i.TaxTotal,
    i.TotalAmount,
    i.Status
FROM dbo.Invoices AS i
INNER JOIN dbo.Customers AS c
    ON i.CustomerId = c.CustomerId;
GO


/* =====================================================
   2. STORED PROCEDURE: Get invoices for a customer
   ===================================================== */

CREATE OR ALTER PROCEDURE dbo.GetInvoicesByCustomer
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.InvoiceId,
        i.InvoiceNumber,
        c.Name AS CustomerName,
        i.InvoiceDate,
        i.DueDate,
        i.Subtotal,
        i.TaxTotal,
        i.TotalAmount,
        i.Status
    FROM dbo.Invoices AS i
    INNER JOIN dbo.Customers AS c
        ON i.CustomerId = c.CustomerId
    WHERE i.CustomerId = @CustomerId
    ORDER BY i.InvoiceDate DESC;
END;
GO


/* =====================================================
   3. TRIGGER: Automatically update invoice payment status
   ===================================================== */

CREATE OR ALTER TRIGGER dbo.trg_Payments_UpdateInvoiceStatus
ON dbo.Payments
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AffectedInvoices TABLE
    (
        InvoiceId INT PRIMARY KEY
    );

    INSERT INTO @AffectedInvoices (InvoiceId)
    SELECT InvoiceId FROM inserted
    UNION
    SELECT InvoiceId FROM deleted;

    UPDATE i
    SET i.Status =
        CASE
            WHEN i.TotalAmount > 0
                 AND ISNULL(p.TotalPaid, 0) >= i.TotalAmount
                THEN 'Paid'
            WHEN ISNULL(p.TotalPaid, 0) > 0
                THEN 'Partially Paid'
            ELSE 'Draft'
        END
    FROM dbo.Invoices AS i
    INNER JOIN @AffectedInvoices AS a
        ON i.InvoiceId = a.InvoiceId
    OUTER APPLY
    (
        SELECT SUM(pay.Amount) AS TotalPaid
        FROM dbo.Payments AS pay
        WHERE pay.InvoiceId = i.InvoiceId
    ) AS p;
END;
GO


/* =====================================================
   4. INDEXES: Improve frequently searched columns
   ===================================================== */

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Invoices_InvoiceNumber'
      AND object_id = OBJECT_ID('dbo.Invoices')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Invoices_InvoiceNumber
        ON dbo.Invoices (InvoiceNumber);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Invoices_CustomerId_InvoiceDate'
      AND object_id = OBJECT_ID('dbo.Invoices')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Invoices_CustomerId_InvoiceDate
        ON dbo.Invoices (CustomerId, InvoiceDate DESC);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Payments_InvoiceId'
      AND object_id = OBJECT_ID('dbo.Payments')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_InvoiceId
        ON dbo.Payments (InvoiceId);
END;
GO

PRINT 'InvoicePro SQL objects created successfully.';
GO
