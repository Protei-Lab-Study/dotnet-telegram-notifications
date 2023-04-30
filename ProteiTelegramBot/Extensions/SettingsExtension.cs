using FluentValidation;
using IL.FluentValidation.Extensions.Options;
using Microsoft.Extensions.Options;
using ProteiTelegramBot.Config;
using ProteiTelegramBot.Config.Validations;
using ProteiTelegramBot.Models;
using Telegram.Bot.Types.Enums;

namespace ProteiTelegramBot.Extensions
{
    public static class SettingsExtension
    {
        public static IServiceCollection AddSettings(this IServiceCollection services,
            ConfigurationManager configuration)
        {
            services.AddSettings<TelegramOptions, TelegramOptionsValidator>(configuration, TelegramOptions.Telegram);
            services.AddSettings<YouTrackOptions, YouTrackOptionsValidator>(configuration, YouTrackOptions.YouTrack);
            services.AddSettings<NotificationCronOptions, NotificationCronOptionsValidator>(configuration, NotificationCronOptions.NotificationCron);
            return services;
        }

        public static IApplicationBuilder ValidateSettings(this IApplicationBuilder app)
        {
            app.ValidateSettings<YouTrackOptions>();
            app.ValidateSettings<TelegramOptions>();
            return app;
        }

        private static void AddSettings<TOptions, TOptionsValidator>(this IServiceCollection services,
            ConfigurationManager configuration,
            string sectionName) where TOptions : class
            where TOptionsValidator : class, IValidator<TOptions>
        {
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionName))
                .Validate<TOptions, TOptionsValidator>();
        }

        private static void ValidateSettings<TOptions>(this IApplicationBuilder app) where TOptions : class
        {
            try
            {
                var _ = app.ApplicationServices.GetService<IOptions<TOptions>>()?.Value;
            }
            catch (Exception e)
            {
                throw new ValidationException($"{nameof(TOptions)} invalid settings due to:{Environment.NewLine}{e}");
                throw;
            }
        }
    }
}