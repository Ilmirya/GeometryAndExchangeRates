using System.Xml.Linq;
using System.Xml.Serialization;
using GeometryAndExchangeRates.Integration;
using Microsoft.AspNetCore.Mvc;

namespace GeometryAndExchangeRates.Features.CurrencyRate
{
    [ApiController]
    [Route("currency-rate")]
    public class CurrencyRateController : ControllerBase
    {
        private readonly DailyInfoSoap _dailyInfoSoap;
        private readonly CurrencyRateSettings _currencyRateSettings;
        
        public CurrencyRateController(DailyInfoSoap dailyInfoSoap, CurrencyRateSettings currencyRateSettings)
        {
            _dailyInfoSoap = dailyInfoSoap;
            _currencyRateSettings = currencyRateSettings;
        }

        [HttpGet]
        public async Task<double> Get(int x, int y)
        {
            if (IsNotInsideCircle(x, y, _currencyRateSettings.Radius))
            {
                throw new ArgumentOutOfRangeException($"Координата не попала в окружность радиуса {_currencyRateSettings.Radius}");
            }

            var date = GetDateFromСoordinates(x, y);
            
            var currencyRate = await FindCurrencyRate(_currencyRateSettings.CurrencyCode, date);
            if (currencyRate is null)
            {
                throw new ArgumentNullException($"Нет данных о курсе {_currencyRateSettings.CurrencyCode} за {date:MM/dd/yyyy}");
            }

            return currencyRate.Rate;
        }

        private static bool IsNotInsideCircle(int x, int y, int radius) => x * x + y * y > radius * radius;

        private static DateTime GetDateFromСoordinates(int x, int y)
        {
            return x switch
            {
                _ when x > 0 && y > 0 => DateTime.Today,
                _ when x > 0 && y < 0 => DateTime.Today.AddDays(1),
                _ when x < 0 && y > 0 => DateTime.Today.AddDays(-1),
                _ when x < 0 && y < 0 => DateTime.Today.AddDays(-2),
                _ => throw new ArgumentOutOfRangeException("Координаты не удовлетворяют условиям")
            };
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
        
        private static ValuteCursOnDate? Deserialize(XNode element)
        {
            var serializer = new XmlSerializer(typeof(ValuteCursOnDate));
            return serializer.Deserialize(element.CreateReader()) as ValuteCursOnDate;
        }
    }
}