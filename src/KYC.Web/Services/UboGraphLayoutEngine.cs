using KYC.Application.Dtos;

namespace KYC.Web.Services;

public sealed record UboGraphLayout(
    IReadOnlyDictionary<Guid, UboNodePosition> Positions,
    double Width,
    double Height);

public sealed record UboNodePosition(double X, double Y, double Width, double Height);

public static class UboGraphLayoutEngine
{
    public const double NodeWidth = 172;
    public const double NodeHeight = 76;
    public const double ColumnGap = 200;
    public const double RowGap = 14;
    public const double Padding = 36;

    public static UboGraphLayout Compute(
        IReadOnlyList<UboGraphNodeDto> nodes,
        int maxDepth,
        bool compact = false)
    {
        var visible = nodes.Where(n => n.Depth <= maxDepth).ToList();
        if (visible.Count == 0)
            return new UboGraphLayout(new Dictionary<Guid, UboNodePosition>(), 400, 200);

        var nodeW = compact ? 140 : NodeWidth;
        var nodeH = compact ? 60 : NodeHeight;
        var colGap = compact ? 160 : ColumnGap;
        var rowGap = compact ? 10 : RowGap;
        var pad = compact ? 24 : Padding;

        var byDepth = visible.GroupBy(n => n.Depth).OrderBy(g => g.Key).ToList();
        var maxRows = byDepth.Max(g => g.Count());
        var height = pad * 2 + maxRows * nodeH + (maxRows - 1) * rowGap;
        var width = pad * 2 + (byDepth.Count * nodeW) + (Math.Max(0, byDepth.Count - 1) * colGap);

        var positions = new Dictionary<Guid, UboNodePosition>();

        foreach (var group in byDepth)
        {
            var list = group.OrderBy(n => n.Name).ToList();
            var colHeight = list.Count * nodeH + (list.Count - 1) * rowGap;
            var startY = pad + (height - pad * 2 - colHeight) / 2;
            var x = pad + group.Key * (nodeW + colGap);

            for (var i = 0; i < list.Count; i++)
            {
                var y = startY + i * (nodeH + rowGap);
                positions[list[i].Id] = new UboNodePosition(x, y, nodeW, nodeH);
            }
        }

        return new UboGraphLayout(positions, width, height);
    }
}
