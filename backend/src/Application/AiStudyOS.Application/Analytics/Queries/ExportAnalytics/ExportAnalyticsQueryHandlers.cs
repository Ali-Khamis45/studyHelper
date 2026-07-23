using AiStudyOS.Application.Analytics.Export;
using AiStudyOS.Application.Analytics.Queries.GetAnalyticsOverview;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.ExportAnalytics;

/// <summary>Both formats render the exact same GetAnalyticsOverviewQuery result — export is a pure rendering step, never a second computation path that could drift from what the UI shows.</summary>
public class ExportAnalyticsPdfQueryHandler(IMediator mediator) : IQueryHandler<ExportAnalyticsPdfQuery, byte[]>
{
    public async ValueTask<byte[]> Handle(ExportAnalyticsPdfQuery query, CancellationToken ct)
    {
        var report = await mediator.Send(new GetAnalyticsOverviewQuery(query.From, query.To), ct);
        return AnalyticsPdfExporter.Generate(report);
    }
}

public class ExportAnalyticsCsvQueryHandler(IMediator mediator) : IQueryHandler<ExportAnalyticsCsvQuery, byte[]>
{
    public async ValueTask<byte[]> Handle(ExportAnalyticsCsvQuery query, CancellationToken ct)
    {
        var report = await mediator.Send(new GetAnalyticsOverviewQuery(query.From, query.To), ct);
        return AnalyticsCsvExporter.Generate(report);
    }
}
