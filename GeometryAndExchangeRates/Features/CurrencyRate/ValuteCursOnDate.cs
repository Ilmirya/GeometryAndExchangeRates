using System.Xml.Serialization;

namespace GeometryAndExchangeRates.Features.CurrencyRate;

public class ValuteCursOnDate
{
    [XmlElement("VchCode")] 
    public string Code { get; set; } = null!;
    
    [XmlElement("VunitRate")]
    public double Rate { get; set; }
    
}