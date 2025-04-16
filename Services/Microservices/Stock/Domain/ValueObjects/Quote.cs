namespace Stock.Domain.ValueObjects;

public sealed record Quote(
    DateTime Day,
    decimal Price,
    string ModelType,
    decimal Confidence,
    string ModelVersion
);