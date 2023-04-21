CREATE PROCEDURE [dbo].[p_top10]
@IMEI varchar(50)

AS

SELECT id, date_track, latitude, longitude,

case when
Datediff(SECOND, '19000101',lead(date_track) over(ORDER BY date_track ASC) - date_track)<30*60 
then
Datediff(SECOND, '19000101',lead(date_track) over(ORDER BY date_track ASC) - date_track)
else 0 end as diff,

case when 
Datepart(minute, date_track - lag(date_track) over(ORDER BY date_track ASC))>=30
or
Datepart(hour, date_track - lag(date_track) over(ORDER BY date_track ASC))>=1
then 1 
else 0 end as startDistance,

case when 
Datepart(minute, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=30
or
Datepart(hour, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=1
then 2 
else 0 end as endDistance,

case when 
Datepart(minute, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=30
or
Datepart(hour, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=1
then NULL 
else lead(latitude) over(ORDER BY date_track ASC) end as latitudeNext,

case when 
Datepart(minute, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=30
or
Datepart(hour, lead(date_track) over(ORDER BY date_track ASC) - date_track)>=1
then NULL 
else lead(longitude) over(ORDER BY date_track ASC) end as longitudeNext

FROM TrackLocation
WHERE IMEI=@IMEI
ORDER BY date_track ASC


GO
