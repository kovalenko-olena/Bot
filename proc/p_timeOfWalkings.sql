CREATE PROCEDURE [dbo].[p_timeOfWalkings]
@IMEI varchar(50)

AS

SELECT SUM(Datediff(SECOND, '19000101', kk.diff))/60 as timeOfWalkings FROM 
(SELECT id, date_track, latitude, longitude,
lead(date_track) over(ORDER BY date_track ASC) - date_track as diff

FROM TrackLocation
WHERE IMEI=@IMEI
)kk

WHERE Datediff(SECOND, '19000101', kk.diff)<(30*60) 

GO