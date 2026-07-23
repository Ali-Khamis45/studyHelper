using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.ExportAnalytics;

public record ExportAnalyticsPdfQuery(DateOnly? From = null, DateOnly? To = null) : IQuery<byte[]>;

public record ExportAnalyticsCsvQuery(DateOnly? From = null, DateOnly? To = null) : IQuery<byte[]>;
