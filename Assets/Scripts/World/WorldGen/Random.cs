﻿using System;


public static class Random
{
    private static int _seed;
    public static int Seed
    {
        get
        {
            return _seed;
        }
        set
        {
            _seed = value;
            rand = new System.Random(_seed);
        }
    }

    private static System.Random rand = new System.Random();

    public static void NewRandomSeed()
    {
        Seed = Environment.TickCount;
    }

    /// <summary>
    /// Returns a random float between 0 and 360
    /// </summary>
    /// <returns></returns>
    public static float RandomDirection()
    {
        return Range(0, 360);
    }

    /// <summary>
    /// Returns a float between these two float
    /// </summary>
    /// <param name="min">Minimum of the range, inclusive </param>
    /// <param name="max">Maximum of the range, exclusive </param>
    /// <returns></returns>
    public static float Range(float min, float max)
    {
        return (float)rand.NextDouble() * (max - min) + min;
    }

    /// <summary>
    /// Returns an int between these two ints
    /// </summary>
    /// <param name="min">Minimum of the range, inclusive</param>
    /// <param name="max">Maximum of the range, exclusive</param>
    /// <returns></returns>
    public static int Range(int min, int max)
    {
        return rand.Next(min, max);
    }
}
