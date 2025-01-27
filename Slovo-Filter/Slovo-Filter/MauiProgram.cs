using Microsoft.Extensions.Logging;
using Slovo_Filter.ViewModel;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
namespace Slovo_Filter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                fonts.AddFont("Roboto-ExtraBold.ttf", "RobotoExtraBold");
                fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
                fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
            });
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<LoginViewModel>();

        // var configuration = new ConfigurationBuilder()
        //     .SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
        //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //     .Build();
        //
        // var connectionString = configuration.GetConnectionString("DefaultConnection");
        // Console.WriteLine(connectionString);
        
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}