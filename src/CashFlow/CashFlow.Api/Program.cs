namespace CashFlow.Api;

using CashFlow.Core.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Register custom services
        builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
        builder.Services.AddScoped<ICashFlowProjectionService, CashFlowProjectionService>();
        builder.Services.AddScoped<ICashFlowForecastService, CashFlowForecastService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
