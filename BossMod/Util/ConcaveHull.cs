using BossMod;
using Clipper2Lib;
public class ConcaveHull
{
    // Euclidean distance between two points
    private static double Distance(WDir a, WDir b) => (b - a).Length();

    // Remove points that are too close to each other based on epsilon value
    private static List<WDir> FilterClosePoints(IEnumerable<WDir> points, double epsilon)
    {
        List<WDir> filteredPoints = [];
        foreach (var point in points)
        {
            if (filteredPoints.All(p => Distance(p, point) > epsilon))
            {
                filteredPoints.Add(point);
            }
        }
        return filteredPoints;
    }

    // Convert floating-point points to integer-based Path64
    private static Path64 ConvertToPath64(List<WDir> points, double scale)
    {
        return new Path64(points.Select(p => new Point64((long)(p.X * scale), (long)(p.Z * scale))).ToList());
    }

    // Convert integer-based Path64 back to floating-point points
    private static List<WDir> ConvertToPoints(Path64 path, double scale)
    {
        return path.Select(p => new WDir((float)(p.X / scale), (float)(p.Y / scale))).ToList();
    }

    public static List<WDir> GenerateConcaveHull(List<WDir> points, double alpha = 1, double epsilon = 1e-6)
    {
        if (points.Count < 4)
            return points;

        points = FilterClosePoints(points, epsilon);
        var scale = 100000.0;
        var inputPath = ConvertToPath64(points, scale);
        var offset = -alpha * scale;

        Paths64 solution = [];

        ClipperOffset co = new();
        co.AddPath(inputPath, JoinType.Miter, EndType.Polygon);
        co.Execute(offset, solution);

        return solution.Count == 0 ? points : ConvertToPoints(solution.First(), scale);
    }
}

