using System.Xml.Linq;
using System.Xml.Serialization;
using Ardalis.ApiEndpoints;
using GeometryAndExchangeRates.Features.CurrencyRate.Core;
using GeometryAndExchangeRates.Integration;
using GeometryAndExchangeRates.Integration.DailyInfoWebServ;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeometryAndExchangeRates.Features.CurrencyRate;

public record Point([FromQuery]int X, [FromQuery] int Y) : IRequest<double>;

public sealed class GetCurrencyRateEndpoint : EndpointBaseAsync.WithRequest<Point>.WithResult<double>
{
    private readonly IMediator _mediator;

    public GetCurrencyRateEndpoint(IMediator mediator) => _mediator = mediator;

    [HttpGet("/currency-rate")]
    public override async Task<double> HandleAsync([FromQuery] Point point, CancellationToken ct = default) 
        => await _mediator.Send(point, ct);
}

internal sealed class GetCurrencyRate: IRequestHandler<Point, double>
{
    private readonly DailyInfoSoap _dailyInfoSoap;
    private readonly CurrencyRateSettings _currencyRateSettings;
        
    public GetCurrencyRate(DailyInfoSoap dailyInfoSoap, CurrencyRateSettings currencyRateSettings)
    {
        _dailyInfoSoap = dailyInfoSoap;
        _currencyRateSettings = currencyRateSettings;
    }
    
    public async Task<double> Handle(Point point, CancellationToken ct)
    {
        if (IsNotInsideCircle(point, _currencyRateSettings.Radius))
        {
            throw new ArgumentOutOfRangeException($"Координата не попала в окружность радиуса {_currencyRateSettings.Radius}");
        }

        var date = GetDateByСoordinates(point);
            
        var currencyRate = await FindCurrencyRate(_currencyRateSettings.CurrencyCode, date);
        if (currencyRate is null)
        {
            throw new ArgumentNullException($"Нет данных о курсе {_currencyRateSettings.CurrencyCode} за {date:MM/dd/yyyy}");
        }

        return currencyRate.Rate;
    }
    
    private async Task<ValuteCursOnDate?> FindCurrencyRate(string code, DateTime date)
    {
        var dailyCurs = await _dailyInfoSoap.GetCursOnDateAsync(date);
        if (dailyCurs is null)
        {
            throw new ArgumentNullException(nameof(dailyCurs));
        }
        var node = dailyCurs.Nodes[1];
        return node.Element("ValuteData").Nodes().Select(Deserialize).FirstOrDefault(x => x.Code == code);
    }
    
    private static bool IsNotInsideCircle(Point point, int radius) 
        =>  point.X * point.X + point.Y * point.Y > radius * radius;
    
    private static DateTime GetDateByСoordinates(Point point)
    {
        return point.X switch
        {
            _ when point.X > 0 && point.Y > 0 => DateTime.Today,
            _ when point.X > 0 && point.Y < 0 => DateTime.Today.AddDays(1),
            _ when point.X < 0 && point.Y > 0 => DateTime.Today.AddDays(-1),
            _ when point.X < 0 && point.Y < 0 => DateTime.Today.AddDays(-2),
            _ => throw new ArgumentOutOfRangeException("Координаты не удовлетворяют условиям")
        };
    }
    
    private static ValuteCursOnDate? Deserialize(XNode element)
    {
        var serializer = new XmlSerializer(typeof(ValuteCursOnDate));
        return serializer.Deserialize(element.CreateReader()) as ValuteCursOnDate;
    }
}