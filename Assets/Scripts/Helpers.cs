using System;
using System.Collections.Generic;
using System.Linq;


public static class Helpers
{
    public static Random Random = new Random();

    public static T PickRandom<T>(this List<T> source)
    {
        return source[Random.Next(source.Count())];
    }

    public static float Truncate(this float value, int digits)
    {
        double mult = Math.Pow(10.0, digits);
        double result = Math.Truncate(mult * value) / mult;
        return (float)result;
    }
}

