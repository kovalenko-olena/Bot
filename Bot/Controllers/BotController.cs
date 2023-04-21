using Bot.Entities;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;

namespace Bot.Controllers
{
	[ApiController]
	[Route("/")]
	public class BotController : ControllerBase
	{
		private readonly IViberBotApi _bot;

		private DbServices _dbServices;


		public BotController(IViberBotApi viberBotApi, IConfiguration config)
		{
			_bot = viberBotApi;
			_dbServices = new DbServices(config);
		}


		[HttpPost]
		public async Task<IActionResult> Post([FromBody] object update)
		{
			var callback = JsonConvert.DeserializeObject<Callback>(update.ToString());

			switch (callback.Event)
			{
				case ViberEventType.Message:
					{
						string responseMessage;
						if (callback.Message.textMessage.StartsWith("ТОП 10 прогулянок"))
						{
							Dictionary<float, string> result10top = _dbServices.GetTop10(callback.Message.textMessage.Split("+").Last());

							result10top = result10top.OrderByDescending(key => key.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
							//Прогулянка 10 500.00 22
							//responseMessage = "\u3164 ⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡\u3164 ⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡⁡\u3164 \u3164 ⁡Назва \u3164 \u3164 \u3164 \t \u3164 км \u3164 \t хв \r";
							responseMessage = " ⁡      Назва       \t   км \t  хв\r";
							int countForPrint = 10;
							foreach (var row in result10top)
							{
								if (countForPrint > 0) responseMessage += $"{row.Value}\r";
								countForPrint--;
							}
							
							await _bot.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.KeyboardMessage
							{
								Receiver = callback.Sender.Id,
								Sender = new ViberUser.User()
								{
									Name = "Bot"
								},
								Text = responseMessage,
								Keyboard = new ViberKeyboard
								{
									HeightScale = 25,
									CustomDefaultHeight = 25,
									BackgroundColor = "#b1faf0",
									Buttons = new List<ViberKeyboardButton>
						{
							new ViberKeyboardButton
							{
								ActionType = "reply",
								ActionBody = "назад",
								Text = "назад",
								TextSize = "regular"
							}
						}
								}
							});

							break;
						}
						if (callback.Message.textMessage == "назад")
						{
							responseMessage = "Введіть IMEI";

							await _bot.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.TextMessage
							{
								Receiver = callback.Sender.Id,
								Sender = new ViberUser.User()
								{
									Name = "Bot"
								},
								Text = responseMessage
							});

							break;
						}

						int countOfWalkings = 0;

						if (!long.TryParse(callback.Message.textMessage, out long imeiLong))
						{
							responseMessage = "Неправильний формат";
						}
						else
						{
							string imei = callback.Message.textMessage;
							countOfWalkings = _dbServices.GetNumberOfWalkings(imei);

							if (countOfWalkings == 0)
							{
								responseMessage = "Неправильний IMEI";
							}
							else
							{
								float allDistance = _dbServices.GetTotalDistance(imei);
								int timeOfWalkings = _dbServices.GetTimeOfWalkings(imei);
								responseMessage = $"Всього прогулянок: {countOfWalkings}\rВсього км нагуляно: " +
											$"{allDistance}\rВсього часу, хв.: {timeOfWalkings}";
							}
						}

						

						await _bot.SendMessageAsync<ViberResponse.SendMessageResponse>(new ViberMessage.KeyboardMessage
						{
							Receiver = callback.Sender.Id,
							Sender = new ViberUser.User()
							{
								Name = "Bot"
							},
							Text = responseMessage,
							Keyboard = countOfWalkings == 0 ? null : new ViberKeyboard
							{
								//DefaultHeight = true,
								HeightScale=25,
								CustomDefaultHeight=25,
								BackgroundColor= "#b1faf0",
								Buttons = new List<ViberKeyboardButton>
					{
						new ViberKeyboardButton
						{
							ActionType = "reply",
							ActionBody = $"ТОП 10 прогулянок+{imeiLong}",
							Text = "ТОП 10 прогулянок",
							TextSize = "regular"
						}
					}
							}
						});

						break;
					}
				// start
				case ViberEventType.ConversationStarted:
					{
						string responseMessage;
						List<long> resultIMEI = _dbServices.GetAllIMEI();

						responseMessage = "ALL IMEI IN DATABASE:\r";

						foreach (var row in resultIMEI)
						{
							responseMessage += $" {row} \r";
						}

						var result = new ViberMessage.TextMessage
						{
							Sender = new ViberUser.User()
							{
								Name = "Bot"
							},
							Text = responseMessage + "\rВведіть IMEI"

						};
						return Ok(result);
					}
			}

			return Ok();
		}





		[HttpGet]
		public async Task<IActionResult> Get()
		{
			return Ok("Viber-bot is active");
		}
	}
}
