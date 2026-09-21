SELECT TOP 10
    VolunteerID,
    FullName,
    Email,
    PhoneNumber,
    Skills,
    Availability,
    RegistrationDate,
    VolunteerStatus
FROM dbo.Volunteers
ORDER BY RegistrationDate DESC;