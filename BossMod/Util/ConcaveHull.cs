using BossMod;
using Clipper2Lib;

public class ConcaveHull
{
    // Remove points that are too close to each other based on epsilon value
    private static List<WPos> FilterClosePoints(IEnumerable<WPos> points, double epsilon)
    {
        List<WPos> filteredPoints = [];
        foreach (var point in points)
        {
            if (filteredPoints.All(p => (p - point).Length() > epsilon))
            {
                filteredPoints.Add(point);
            }
        }
        return filteredPoints;
    }

    // Convert floating-point points to integer-based Path64
    private static Path64 ConvertToPath64(List<WPos> points, double scale)
    {
        return new Path64(points.Select(p => new Point64((long)(p.X * scale), (long)(p.Z * scale))).ToList());
    }

    // Convert integer-based Path64 back to floating-point points
    private static List<WPos> ConvertToPoints(Path64 path, double scale)
    {
        return path.Select(p => new WPos((float)(p.X / scale), (float)(p.Y / scale))).ToList();
    }

    public static List<WPos> GenerateConcaveHull(List<WPos> points, float alpha, float epsilon)
    {
        if (points.Count < 3)
            return new List<WPos>(points); // Not enough points to form a polygon

        // Filter points that are too close to each other
        points = FilterClosePoints(points, epsilon);

        // Set the scale factor for precision
        double scale = 100000.0;

        // Convert points to Path64 (integer-based)
        Path64 inputPath = ConvertToPath64(points, scale);

        // Create the ClipperOffset object without applying an offset
        ClipperOffset co = new ClipperOffset();
        co.AddPath(inputPath, JoinType.Miter, EndType.Polygon);

        // Prepare a solution container
        Paths64 solution = [];

        // Perform the Clipper operation
        co.Execute(0, solution); // No offset applied here, just process the input path

        // If there's no solution or the resulting path is empty, return the original points
        if (solution.Count == 0)
            return points;

        // Ensure no holes are present by merging all the resulting polygons into one
        // Only take the outermost polygons (which should have a clockwise orientation)
        var mergedPolygon = MergePaths(solution);

        // Convert the resulting Path64 back to floating-point Point structure
        var hull = ConvertToPoints(mergedPolygon, scale);

        // Final step: Ensure the edges between points in the hull respect the Alpha distance threshold
        hull = ApplyAlphaFilter(hull, alpha);

        return hull;
    }

    // Merges paths to ensure no holes remain in the polygon
    private static Path64 MergePaths(Paths64 paths)
    {
        // Clipper object to perform union operations
        var clipper = new Clipper64();

        // Add the paths to the clipper object
        clipper.AddPaths(paths, PathType.Subject);

        // Solution to hold the merged result
        Paths64 mergedSolution = [];

        // Perform the union operation to merge the polygons
        clipper.Execute(ClipType.Union, FillRule.NonZero, mergedSolution);

        // Return the largest outer path to ensure the final shape contains no holes
        return mergedSolution.OrderByDescending(Clipper.Area).First();
    }

    // Filter edges to ensure that no edge in the hull is longer than the Alpha value
    private static List<WPos> ApplyAlphaFilter(List<WPos> hull, double alpha)
    {
        List<WPos> filteredHull = [hull[0]];

        for (var i = 1; i < hull.Count; i++)
        {
            var currentPoint = hull[i];
            var lastAddedPoint = filteredHull.Last();

            // Add point only if distance from the last added point is greater than alpha
            if ((currentPoint - lastAddedPoint).Length() > alpha)
            {
                filteredHull.Add(currentPoint);
            }
        }

        // Make sure to close the hull by checking the distance between the last and first points
        if ((filteredHull.Last() - filteredHull.First()).Length() > alpha)
        {
            filteredHull.Add(filteredHull.First());
        }

        return filteredHull;
    }

    public static WPos CalculateCentroid(List<WPos> points)
    {
        if (points == null || points.Count == 0)
            return new WPos(0, 0); // Return zero vector if no points

        float sumX = 0, sumZ = 0;
        foreach (var point in points)
        {
            sumX += point.X;
            sumZ += point.Z;
        }

        float centerX = sumX / points.Count;
        float centerZ = sumZ / points.Count;

        return new WPos(centerX, centerZ);
    }
}
