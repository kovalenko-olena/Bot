CREATE PROCEDURE [dbo].[p_allIMEI]
AS

SELECT distinct IMEI as imei  
FROM 
TrackLocation

GO