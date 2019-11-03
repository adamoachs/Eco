using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract base class that every Gene in Eco derives from. Genes control behavior and traits of creatures, and are derived from their parents' genes
/// </summary>
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

    /// <summary>
    /// Return a gene of either parent, chosen at random
    /// </summary>
    /// <param name="ParentA">ParentA's gene</param>
    /// <param name="ParentB">ParentB's gene</param>
    /// <returns></returns>
    public static Gene Breed(Gene ParentA, Gene ParentB)
    {
        if(ParentA.GetType() != ParentB.GetType())
            throw new System.ArgumentException("Attempt to cross different genes");

        //Pick at random either parent's gene
        int rnd = Random.Range(0, 2);
        Gene newGene = rnd == 0 ? ParentA : ParentB;

        //Add a random mutation factor. This may need adjustment later. Perhaps use a normal distrubtion instead of uniform distribution for the mutation amount
        newGene.Value += Random.Range(-0.1F, 0.1F);
        return newGene;
    }
}
