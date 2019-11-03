using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Various utility functions
/// </summary>
public static class Utility
{
    /// <summary>
    /// Calculate n-th percentile of a given range of numbers
    /// </summary>
    /// <param name="seq">Sequence of numbers</param>
    /// <param name="n">Percentile to calculate, between 0F and 1F</param>
    /// <returns>Value at n-th percentile</returns>
    public static float Percentile(IEnumerable<float> seq, float n)
    {
        var elements = seq.ToArray();
        Array.Sort(elements);
        float realIndex = n * (elements.Length - 1);
        int index = (int)realIndex;
        float frac = realIndex - index;
        if (index + 1 < elements.Length)
            return elements[index] * (1 - frac) + elements[index + 1] * frac;
        else
            return elements[index];
    }

    /// <summary>
    /// Calculate n-th percentile of a given range of numbers
    /// </summary>
    /// <param name="seq">Sequence of numbers</param>
    /// <param name="n">Percentile to calculate, between 0F and 1F</param>
    /// <returns>Value at n-th percentile</returns>
    public static double Percentile(IEnumerable<double> seq, float n)
    {
        var elements = seq.ToArray();
        Array.Sort(elements);
        double realIndex = n * (elements.Length - 1);
        int index = (int)realIndex;
        double frac = realIndex - index;
        if (index + 1 < elements.Length)
            return elements[index] * (1 - frac) + elements[index + 1] * frac;
        else
            return elements[index];
    }
}
