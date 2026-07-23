namespace AiStudyOS.Application.Analytics.Dtos.Charts;

/// <summary>Generic (label, value) point — used for Line, Area, and Bar charts on the frontend; which chart type renders it is a frontend concern, not a backend one.</summary>
public record ChartPointDto(string Label, double Value);

public record PieSliceDto(string Label, double Value);

/// <summary>Date as "yyyy-MM-dd" so the frontend can key a GitHub-style contribution heatmap directly off it.</summary>
public record HeatmapCellDto(string Date, int Value);

public record RadarAxisDto(string Axis, double Value);

public record TimelineEventDto(DateTime OccurredAtUtc, string Type, string Label);

public record DistributionBucketDto(string Bucket, int Count);
