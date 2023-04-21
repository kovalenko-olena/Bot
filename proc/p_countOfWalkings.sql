CREATE PROCEDURE [dbo].[p_countOfWalkings]
@IMEI varchar(50)

AS

SELECT count(kk.endDistance) FROM 
(SELECT id, date_track, latitude, longitude,
date_track - lag(date_track) over(ORDER BY date_track ASC) as diff,
lead(date_track) over(ORDER BY date_track ASC) - date_track as diff1,
case when Datepart(minute, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=30
or
Datepart(hour, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=1
then 2 else 0 end
as endDistance
FROM TrackLocation
WHERE IMEI=@IMEI
)kk

GO