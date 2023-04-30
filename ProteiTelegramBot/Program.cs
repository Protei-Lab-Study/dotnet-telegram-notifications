using System.Net.Http.Headers;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProteiTelegramBot.Config;
using ProteiTelegramBot.Exceptions;
using ProteiTelegramBot.Extensions;
using ProteiTelegramBot.Repository;
using ProteiTelegramBot.Services;
using ProteiTelegramBot.Utils;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSettings(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddScoped<INotifier, Notifier>();
builder.Services.AddScoped<IEmployeRepository, EmployeRepository>();

builder.Services.AddSingleton<EmergencyDutyUpdater>();
builder.Services.AddScoped<IDutyNotifier, DutyTelegramNotifier>();
builder.Services.AddScoped<IDutyExtractor, DutyYouTrackExtractor>();
builder.Services.AddScoped<IDutyService, DutyService>();
builder.Services.AddScoped<IDutyRepository, DutyRepository>();
builder.Services.AddScoped<IDutyInformationRepository, DutyInformationRepository>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection")));

builder.Services.AddHttpClient<IDutyExtractor, DutyYouTrackExtractor>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["YouTrack:Url"]);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["YouTrack:Token"]);
});

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>(httpClient =>
    {
        TelegramBotClientOptions options = new(builder.Configuration["Telegram:BotToken"]);
        return new TelegramBotClient(options, httpClient);
    });

// Add Hangfire services. Database manual create: CREATE DATABASE dotnettelegrambot_hangfire;
GlobalConfiguration.Configuration.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection"));
builder.Services.AddHangfire(_ => { });

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed((_) => true)
            .AllowCredentials());
});

var app = builder.Build();
app.Logger.LogInformation(ConfigurationSerializer.Serialize(app.Configuration).ToString());
app.ValidateSettings();
await app.MigrateDatabase<DataContext>();

app.UseJsonExceptionHandler();
app.UseForwardedHeaders();

// Cron YouTrack Duty
var emergencyDutyUpdater = app.Services.GetService<EmergencyDutyUpdater>();
var notificationCronOptions = app.Services.GetService<IOptions<NotificationCronOptions>>();

RecurringJob.AddOrUpdate(
    nameof(EmergencyDutyUpdater.JobId),
    () => emergencyDutyUpdater!.UpdateEmergencyDutyAsync(),
    Cron.Daily(notificationCronOptions!.Value.NotifyDutyAtHourUtc));

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseEndpoints(
    endpoints => { endpoints.MapControllers(); }
);
app.Run();

