SELECT TOP 10
    rpu.ProjectUpdateID,
    rp.ProjectName,
    e.FullName AS PostedBy,
    rpu.UpdateTitle,
    rpu.UpdateContent,
    rpu.PostedAt
FROM dbo.ReliefProjectUpdates rpu
INNER JOIN dbo.ReliefProjects rp
    ON rpu.ReliefProjectID = rp.ReliefProjectID
INNER JOIN dbo.Employees e
    ON rpu.EmployeeID = e.EmployeeID
ORDER BY rpu.PostedAt DESC;