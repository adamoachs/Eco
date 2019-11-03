using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gene {

    /// <summary>
    /// Gene value between 0 and 1
    /// </summary>
    public float Value
    {
        get { return Value; }
        set
        {
            value = Mathf.Clamp01(value);
        }
    }

    public static Gene Breed(Gene ParentA, Gene ParentB)
    {
        int rnd = Random.Range(0, 2);
        Gene newGene = rnd == 0 ? ParentA : ParentB;
        newGene.Value += Random.Range(-0.1F, 0.1F);
        return newGene;
    }
}
