using System;
using System.Collections.Generic;
using System.Linq;


public static class Helpers
{
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
        return radians * (180.0f / (float)Math.PI));
    }
}

