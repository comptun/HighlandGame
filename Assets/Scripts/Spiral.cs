using System;
using System.Collections.Generic;
using UnityEngine;

public static class Spiral
{
    // Enumerates integer grid points in an outward spiral starting at (0,0),
    // yielding only those within Euclidean radius R (x^2 + y^2 <= R^2).
    public static IEnumerable<Vector2Int> PointsInRadius(int R)
    {
        if (R < 0) yield break;

        int x = 0, y = 0;
        int r2 = R * R;

        yield return new Vector2Int(0, 0);

        // Directions: right, up, left, down
        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };

        int stepLen = 1;

        while (true)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                int moves = stepLen;

                for (int i = 0; i < moves; i++)
                {
                    x += dx[dir];
                    y += dy[dir];

                    // Filter to points within the radius
                    if (x * x + y * y <= r2)
                        yield return new Vector2Int(x, y);
                }

                // Increase step length after moving up and after moving down
                // (i.e., after dir==1 and dir==3)
                if (dir % 2 == 1)
                    stepLen++;
            }

            // Stop once the spiral's bounding square has expanded past the circle.
            // After increments, the current max |x| or |y| reachable is about stepLen-1.
            int squareRadius = stepLen - 1;
            if (squareRadius > R)
                yield break;
        }
    }
}