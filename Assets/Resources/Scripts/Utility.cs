using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utility
{
    public static float Percentile(IEnumerable<float> seq, float percentile)
    {
        var elements = seq.ToArray();
        Array.Sort(elements);
        float realIndex = percentile * (elements.Length - 1);
        int index = (int)realIndex;
        float frac = realIndex - index;
        if (index + 1 < elements.Length)
            return elements[index] * (1 - frac) + elements[index + 1] * frac;
        else
            return elements[index];
    }

    public static double Percentile(IEnumerable<double> seq, float percentile)
    {
        var elements = seq.ToArray();
        Array.Sort(elements);
        double realIndex = percentile * (elements.Length - 1);
        int index = (int)realIndex;
        double frac = realIndex - index;
        if (index + 1 < elements.Length)
            return elements[index] * (1 - frac) + elements[index + 1] * frac;
        else
            return elements[index];
    }
}
