using GeometryAndExchangeRates.Features.CurrencyRate.Core;
using GeometryAndExchangeRates.Integration;
using GeometryAndExchangeRates.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog(logger);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.EnableAnnotations();
    });

builder.Services.AddTransient<DailyInfoSoap, DailyInfoSoapClient>(
    _ => new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap));

builder.Services.RegisterCurrencyRate(builder.Configuration);

builder.Services.AddMediatR(config
    => config.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
