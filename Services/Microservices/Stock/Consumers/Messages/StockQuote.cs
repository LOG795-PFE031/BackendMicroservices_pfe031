using System.Text.Json.Serialization;

namespace Stock.Consumers.Messages;

public class StockQuote
{
    [JsonPropertyName("Symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public decimal Price { get; set; }

    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("CorrelationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("MessageType")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("ModelType")]
    public string ModelType { get; set; } = string.Empty;

    [JsonPropertyName("Confidence")]
    public decimal Confidence { get; set; }

    [JsonPropertyName("ModelVersion")]
    public string ModelVersion { get; set; } = string.Empty;
}