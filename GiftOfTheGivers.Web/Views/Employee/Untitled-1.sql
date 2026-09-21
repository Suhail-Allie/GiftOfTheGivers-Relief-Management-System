/* =========================================================
   PROTOTYPE EMPLOYEE + RELIEF PROJECT SEED DATA
   ========================================================= */

DECLARE @EmployeeEmail NVARCHAR(255) =
    N'employee@giftofthegivers.local';

DECLARE @UserID INT;
DECLARE @EmployeeID INT;

-- =========================================================
-- 1. CREATE MATCHING APPLICATION USER PROFILE
-- =========================================================

SELECT @UserID = UserID
FROM dbo.Users
WHERE Email = @EmployeeEmail;

IF @UserID IS NULL
BEGIN
    INSERT INTO dbo.Users
    (
        Email,
        PasswordHash,
        UserRole
    )
    VALUES
    (
        @EmployeeEmail,
        N'Authentication managed by ASP.NET Identity',
        N'Employee'
    );

    SET @UserID = SCOPE_IDENTITY();
END;


-- =========================================================
-- 2. CREATE EMPLOYEE PROFILE
-- =========================================================

SELECT @EmployeeID = EmployeeID
FROM dbo.Employees
WHERE UserID = @UserID;

IF @EmployeeID IS NULL
BEGIN
    INSERT INTO dbo.Employees
    (
        UserID,
        FullName,
        JobTitle
    )
    VALUES
    (
        @UserID,
        N'Prototype Employee',
        N'Relief Operations Employee'
    );

    SET @EmployeeID = SCOPE_IDENTITY();
END;


-- =========================================================
-- 3. CREATE A PROTOTYPE ACTIVE RELIEF PROJECT
-- =========================================================

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ReliefProjects
    WHERE ProjectName = N'Prototype Relief Project'
)
BEGIN
    INSERT INTO dbo.ReliefProjects
    (
        ProjectName,
        Description,
        Location,
        StartDate,
        EndDate,
        ProjectStatus,
        CreatedByEmployeeID
    )
    VALUES
    (
        N'Prototype Relief Project',
        N'Demonstration project used to test relief-project updates in the Part 1 prototype.',
        N'South Africa',
        CAST(SYSUTCDATETIME() AS DATE),
        NULL,
        N'Active',
        @EmployeeID
    );
END;


-- =========================================================
-- VERIFY
-- =========================================================

SELECT
    EmployeeID,
    UserID,
    FullName,
    JobTitle,
    CreatedAt
FROM dbo.Employees;

SELECT
    ReliefProjectID,
    ProjectName,
    Description,
    Location,
    StartDate,
    EndDate,
    ProjectStatus,
    CreatedByEmployeeID,
    CreatedAt
FROM dbo.ReliefProjects;