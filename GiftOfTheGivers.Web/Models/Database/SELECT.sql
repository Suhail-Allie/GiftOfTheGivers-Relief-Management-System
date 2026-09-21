SELECT
    u.Email,
    r.Name AS RoleName
FROM dbo.AspNetUsers u
INNER JOIN dbo.AspNetUserRoles ur
    ON u.Id = ur.UserId
INNER JOIN dbo.AspNetRoles r
    ON ur.RoleId = r.Id
WHERE u.Email = 'donor.azuretest@giftofthegivers.local';