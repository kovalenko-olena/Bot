using Microsoft.OpenApi.Models;
using Viber.Bot.NetCore.Middleware;


namespace Bot
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bot", Version = "v1" });
			});

			builder.Configuration.AddUserSecrets("471ac0de-e8f8-45f6-b9d4-fe37efb105e9");

			builder.Services.AddViberBotApi(opt =>
			{
				opt.Token = builder.Configuration.GetValue<string>("Token");
				opt.Webhook = builder.Configuration.GetValue<string>("Webhook");
			});

			// Add services to the container.
			var connectionString = builder.Configuration
				.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bot v1"));
			app.UseCors(x => { x.AllowAnyHeader(); x.AllowAnyMethod(); x.AllowAnyOrigin(); });
			
			app.UseHttpsRedirection();

		    app.UseAuthorization();

			app.MapControllers();

			app.Run();
			
		}
	}
}