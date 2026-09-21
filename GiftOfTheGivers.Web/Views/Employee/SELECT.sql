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