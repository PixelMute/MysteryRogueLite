using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helpers
{
    public static void DrawDebugLine(int x, int z)
    {
        var rng = UnityEngine.Random.ColorHSV();
        DrawDebugLine(x, z, rng);
    }

    public static void DrawDebugLine(int x, int z, Color color)
    {
        UnityEngine.Debug.DrawLine(new Vector3(x, 0, z), new Vector3(x, 10, z), color, 1000f);
    }

    public static T PickRandom<T>(this List<T> source)
    {
        return source[Random.Range(0, source.Count())];
    }

    public static float Truncate(this float value, int digits)
    {
        double mult = Math.Pow(10.0, digits);
        double result = Math.Truncate(mult * value) / mult;
        return (float)result;
    }

    public static float DegreesToRadians(this float degrees)
    {
        return degrees * ((float)Math.PI / 180f);
    }

    public static float RadiansToDegrees(this float radians)
    {
        return radians * (180.0f / (float)Math.PI);
    }

    public static int Gate(this int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        else if (value > max)
        {
            return max;
        }
        return value;
    }
}

