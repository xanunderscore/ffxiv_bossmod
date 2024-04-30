﻿namespace BossMod.Pathfinding;

// 'map' used for running pathfinding algorithms
// this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
// area covered by each pixel can be in one of the following states:
// - default: safe to traverse but non-goal
// - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
// - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
// - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
// typically we try to find a path to goal with highest priority; if that fails, try lower priorities; if no paths can be found (e.g. we're currently inside an imminent aoe) we find direct path to closest safe pixel
public class Map
{
    public struct Pixel
    {
        public float MaxG; // MaxValue if not dangerous
        public int Priority; // >0 if goal
    }

    public float Resolution { get; private init; } // pixel size, in world units
    public int Width { get; private init; } // always even
    public int Height { get; private init; } // always even
    public Pixel[] Pixels;

    public WPos Center { get; private set; } // position of map center in world units
    public Angle Rotation { get; private init; } // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
    private WDir LocalZDivRes { get; init; }

    public float MaxG { get; private set; } // maximal 'maxG' value of all blocked pixels
    public int MaxPriority { get; private set; } // maximal 'priority' value of all goal pixels

    //public float Speed = 6; // used for converting activation time into max g-value: num world units that player can move per second

    public Pixel this[int x, int y] => InBounds(x, y) ? Pixels[y * Width + x] : new() { MaxG = float.MaxValue, Priority = 0 };

    public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = new())
    {
        Resolution = resolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth / resolution);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight / resolution);
        Pixels = Utils.MakeArray(Width * Height, new Pixel() { MaxG = float.MaxValue, Priority = 0 });

        Center = center;
        Rotation = rotation;
        LocalZDivRes = rotation.ToDirection() / Resolution;
    }

    public Map Clone(WPos center)
    {
        var res = (Map)MemberwiseClone();
        res.Pixels = new Pixel[Pixels.Length];
        Array.Copy(Pixels, res.Pixels, Pixels.Length);
        res.Center = center;
        return res;
    }

    public Vector2 WorldToGridFrac(WPos world)
    {
        var offset = world - Center;
        var x = offset.Dot(LocalZDivRes.OrthoL());
        var y = offset.Dot(LocalZDivRes);
        return new(Width / 2 + x, Height / 2 + y);
    }

    public (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));
    public (int x, int y) ClampToGrid((int x, int y) pos) => (Math.Clamp(pos.x, 0, Width - 1), Math.Clamp(pos.y, 0, Height - 1));
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public WPos GridToWorld(int gx, int gy, float fx, float fy)
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        float ax = (gx - Width / 2 + fx) * rsq;
        float az = (gy - Height / 2 + fy) * rsq;
        return Center + ax * LocalZDivRes.OrthoL() + az * LocalZDivRes;
    }

    // block all pixels for which function returns value smaller than threshold ('inside' shape + extra cushion)
    public void BlockPixelsInside(Func<WPos, float> shape, float maxG, float threshold)
    {
        MaxG = MathF.Max(MaxG, maxG);
        foreach (var (x, y, center) in EnumeratePixels())
        {
            if (shape(center) <= threshold)
            {
                ref var pixel = ref Pixels[y * Width + x];
                pixel.MaxG = MathF.Min(pixel.MaxG, maxG);
            }
        }
    }

    public int AddGoal(int x, int y, int deltaPriority)
    {
        ref var pixel = ref Pixels[y * Width + x];
        pixel.Priority += deltaPriority;
        MaxPriority = Math.Max(MaxPriority, pixel.Priority);
        return pixel.Priority;
    }

    public int AddGoal(Func<WPos, float> shape, float threshold, int minPriority, int deltaPriority)
    {
        int maxAdjustedPriority = minPriority;
        foreach (var (x, y, center) in EnumeratePixels())
        {
            if (shape(center) <= threshold)
            {
                ref var pixel = ref Pixels[y * Width + x];
                if (pixel.Priority >= minPriority)
                {
                    pixel.Priority += deltaPriority;
                    maxAdjustedPriority = Math.Max(maxAdjustedPriority, pixel.Priority);
                }
            }
        }
        MaxPriority = Math.Max(MaxPriority, maxAdjustedPriority);
        return maxAdjustedPriority;
    }

    public IEnumerable<(int x, int y, int priority)> Goals()
    {
        int index = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; ++x)
            {
                if (Pixels[index].MaxG == float.MaxValue)
                    yield return (x, y, Pixels[index].Priority);
                ++index;
            }
        }
    }

    public IEnumerable<(int x, int y, WPos center)> EnumeratePixels()
    {
        var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
        var dx = LocalZDivRes.OrthoL() * rsq;
        var dy = LocalZDivRes * rsq;
        var cy = Center + (-Width / 2 + 0.5f) * dx + (-Height / 2 + 0.5f) * dy;
        for (int y = 0; y < Height; y++)
        {
            var cx = cy;
            for (int x = 0; x < Width; ++x)
            {
                yield return (x, y, cx);
                cx += dx;
            }
            cy += dy;
        }
    }

    // enumerate pixels along line starting from (x1, y1) to (x2, y2); first is not returned, last is returned
    public IEnumerable<(int x, int y)> EnumeratePixelsInLine(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        if (dx >= dy)
        {
            int err = 2 * dy - dx;
            do
            {
                x1 += sx;
                yield return (x1, y1);
                if (err > 0)
                {
                    y1 += sy;
                    yield return (x1, y1);
                    err -= 2 * dx;
                }
                err += 2 * dy;
            }
            while (x1 != x2);
        }
        else
        {
            int err = 2 * dx - dy;
            do
            {
                y1 += sy;
                yield return (x1, y1);
                if (err > 0)
                {
                    x1 += sx;
                    yield return (x1, y1);
                    err -= 2 * dy;
                }
                err += 2 * dx;
            }
            while (y1 != y2);
        }
    }
}
