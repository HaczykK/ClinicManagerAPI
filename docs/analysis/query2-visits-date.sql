SET STATISTICS IO ON;
SET STATISTICS PROFILE ON;

DECLARE @dayStart datetime2 = CAST(GETDATE() AS date);
DECLARE @dayEnd   datetime2 = DATEADD(day, 1, @dayStart);

SELECT Id, Date, Status, PatientId, AssignedDoctorId
FROM Visits
WHERE Date >= @dayStart AND Date < @dayEnd;
