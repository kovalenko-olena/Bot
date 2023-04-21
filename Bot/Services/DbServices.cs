using Bot.Entities;
using Dapper;
using Nest;
using System.Data;
using System.Data.SqlClient;

namespace Bot.Services
{
	public class DbServices
	{
		private IConfiguration _configuration;
		public DbServices(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public int GetProcedureSingleData(string imei, string procedureName)
		{
			try
			{
				using (SqlConnection connection = new(_configuration["ConnectionStrings:DefaultConnection"]))
				{
					var param = new DynamicParameters();
					param.Add("@IMEI", imei, DbType.String, ParameterDirection.Input);

					int result = connection.QueryFirst<int>(procedureName, param, commandType: CommandType.StoredProcedure);

					return result;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		public Dictionary<float, string> GetTop10(string imei)
		{
			try
			{
				using (SqlConnection connection = new(connectionString: _configuration["ConnectionStrings:DefaultConnection"]))
				{
					var param = new DynamicParameters();
					string spName = "p_top10";
					param.Add("@IMEI", imei, DbType.String, ParameterDirection.Input);

					var resultDb = connection.QueryMultiple(spName, param, commandType: CommandType.StoredProcedure).Read();

					// float = distance, string = name | distance | minutes, 
					Dictionary<float, string> resultDictionary = new Dictionary<float, string>();
					float sumDistance = 0;
					int countWalk = 0;
					double sumSec = 0;
					foreach (IDictionary<string, object> row in resultDb)
					{
						decimal longitudeNext = 0;
						decimal latitudeNext = 0;
						decimal longitude1 = 0;
						decimal latitude1 = 0;
						int sec = 0;
						float distance = 0;
						foreach (var pair in row)
						{
							switch (pair.Key)
							{
								case "longitudeNext":
									{
										if (!string.IsNullOrEmpty(pair.Key) && (pair.Value != null))
										{
											longitudeNext = (decimal)pair.Value;
										}
										break;
									}
								case "latitudeNext":
									{
										if (!string.IsNullOrEmpty(pair.Key) && (pair.Value != null))
										{
											latitudeNext = (decimal)pair.Value;
										}
										break;
									}
								case "longitude":
									{
										if (!string.IsNullOrEmpty(pair.Key) && (pair.Value != null))
										{
											longitude1 = (decimal)pair.Value;
										}
										break;
									}
								case "latitude":
									{
										if (!string.IsNullOrEmpty(pair.Key) && (pair.Value != null))
										{
											latitude1 = (decimal)pair.Value;
										}
										break;
									}
								case "sec":
									{
										if (!string.IsNullOrEmpty(pair.Key) && (pair.Value != null))
										{
											sec = (int)pair.Value;
										}
										break;
									}

								default: break;
							}
						}
						if (longitudeNext != 0 && latitudeNext != 0)
						{
							GeoCoordinate geoCoordinate1 = new((double)latitude1, (double)longitude1);
							GeoCoordinate geoCoordinate2 = new((double)latitudeNext, (double)longitudeNext);
							if (geoCoordinate1 != geoCoordinate2)
							{
								distance = (float)GetDistanceInKm(geoCoordinate1, geoCoordinate2);
							}
							sumDistance += distance;
							sumSec += sec;
						}
						// the end of distance 
						if (longitudeNext == 0 && latitudeNext == 0 && sumDistance != 0)
						{
							countWalk = countWalk + 1;
							try
							{
								bool przNotAddInDictionary = false;
								foreach (var key in resultDictionary)
								{
									if (key.Key == sumDistance) przNotAddInDictionary = true;
								}
								if (!przNotAddInDictionary)
								{
									string txtForDictionary = "";
									if (countWalk > 9)
										txtForDictionary = "Прогулянка " + countWalk;
									else
										txtForDictionary = "Прогулянка  " + countWalk;
									txtForDictionary += " \t ";
									if (sumDistance >= 100)
										txtForDictionary += Math.Round(sumDistance, 2);

									else if (sumDistance >= 10)
										if (Math.Round(sumDistance, 3) == Math.Round(sumDistance, 2))
											txtForDictionary += Math.Round(sumDistance, 3) + "0";
										else
											txtForDictionary += Math.Round(sumDistance, 3);

									else
										txtForDictionary += Math.Round(sumDistance, 4);
									txtForDictionary += " \t ";
									txtForDictionary += Math.Round(sumSec / 60);
									resultDictionary.Add(sumDistance, txtForDictionary);
								}
							}
							catch (Exception) { }
						}
						if (longitudeNext == 0 && latitudeNext == 0)
						{
							sumDistance = 0;
							sumSec = 0;
						}
					}
					return resultDictionary;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}



		public float GetTotalDistance(string imei)
		{
			try
			{
				using (SqlConnection connection = new(connectionString: _configuration["ConnectionStrings:DefaultConnection"]))
				{
					var param = new DynamicParameters();
					string spName = "p_totalDistance";
					param.Add("@IMEI", imei, DbType.String, ParameterDirection.Input);

					var resultDb = connection.QueryMultiple(spName, param, commandType: CommandType.StoredProcedure).Read();
					float totalDistance = 0;

					foreach (IDictionary<string, object> row in resultDb)
					{
						decimal longitudeNext = 0;
						decimal latitudeNext = 0;
						decimal longitude1 = 0;
						decimal latitude1 = 0;
						foreach (var pair in row)
						{
							if (!string.IsNullOrEmpty(pair.Key) && (pair.Key) == "longitudeNext" && (pair.Value != null))
							{
								longitudeNext = (decimal)pair.Value;
							}
							if (!string.IsNullOrEmpty(pair.Key) && (pair.Key) == "latitudeNext" && (pair.Value != null))
							{
								latitudeNext = (decimal)pair.Value;
							}
							if (!string.IsNullOrEmpty(pair.Key) && (pair.Key) == "longitude" && (pair.Value != null))
							{
								longitude1 = (decimal)pair.Value;
							}
							if (!string.IsNullOrEmpty(pair.Key) && (pair.Key) == "latitude" && (pair.Value != null))
							{
								latitude1 = (decimal)pair.Value;
							}
						}
						if (longitudeNext != 0 && latitudeNext != 0)
						{
							GeoCoordinate geoCoordinate1 = new((double)latitude1, (double)longitude1);
							GeoCoordinate geoCoordinate2 = new((double)latitudeNext, (double)longitudeNext);
							if (geoCoordinate1 != geoCoordinate2)
							{
								totalDistance = totalDistance + (float)GetDistanceInKm(geoCoordinate1, geoCoordinate2);
							}
						}
					}

					return totalDistance;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}


		public int GetTimeOfWalkings(string imei)
		{
			string spName = "p_timeOfWalkings";
			return GetProcedureSingleData(imei, spName);
		}

		public int GetNumberOfWalkings(string imei)
		{
			string spName = "p_countOfWalkings";
			return GetProcedureSingleData(imei, spName);
		}

		public List<long> GetAllIMEI()
		{
			try
			{
				using (SqlConnection connection = new(connectionString: _configuration["ConnectionStrings:DefaultConnection"]))
				{
					var param = new DynamicParameters();
					string spName = "p_allIMEI";

					var resultDb = connection.QueryMultiple(spName, commandType: CommandType.StoredProcedure).Read();

					List<long> resultList = new();
					long imei = 0;

					foreach (IDictionary<string, object> row in resultDb)
					{

						foreach (var pair in row)
						{
							if (!string.IsNullOrEmpty(pair.Key) && (pair.Key) == "imei" && (pair.Value != null))
							{
								try
								{
									//imei = long.Parse(pair.Value);
									imei = Convert.ToInt64(pair.Value);
								}
								catch (Exception) { }
							}


						}
						resultList.Add(imei);
					}
					return resultList;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}


		public static double GetDistanceInKm(GeoCoordinate point1, GeoCoordinate point2)
		{
			var R = 6371d;
			var dLat = Deg2Rad(point2.Latitude - point1.Latitude);
			var dLon = Deg2Rad(point2.Longitude - point1.Longitude);
			var a =
				Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
				Math.Cos(Deg2Rad(point1.Latitude)) * Math.Cos(Deg2Rad(point2.Latitude)) *
				Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
			var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
			var d = R * c;
			return d;
		}

		private static double Deg2Rad(double deg)
		{
			return deg * (Math.PI / 180d);
		}
	}
}

