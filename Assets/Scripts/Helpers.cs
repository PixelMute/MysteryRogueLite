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
}

