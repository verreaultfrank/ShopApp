namespace JobHunter.Domain.Models;

public class AnalyticsQueryConfig
{
    public string PrimaryTable { get; set; } = string.Empty;
    public string? JoinTable { get; set; }
    public JoinType JoinType { get; set; } = JoinType.Inner;
    public string? JoinColumnLeft { get; set; }
    public string? JoinColumnRight { get; set; }
    public string XAxisColumn { get; set; } = string.Empty;
    public string YAxisColumn { get; set; } = string.Empty;
    public AggregateFunction AggregateFunction { get; set; } = AggregateFunction.Count;
}

public enum JoinType
{
    Inner,
    Left,
    Right,
    Full
}

public enum AggregateFunction
{
    None,
    Count,
    Sum,
    Avg,
    Min,
    Max
}
