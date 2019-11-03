using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

/// <summary>
/// An abstract base class that every living creature in Eco derives from
/// </summary>
public abstract class Creature : MonoBehaviour {

    /// <summary>
    /// Breed to creatures together
    /// </summary>
    /// <param name="ParentA">ParentA</param>
    /// <param name="ParentB">ParentB</param>
    /// <returns>New child creature of seem type</returns>
	public static Creature Breed(Creature ParentA, Creature ParentB)
    {
        //Don't allow cross-breeding. Maybe in the future this could be allowed with select species?
        if (ParentA.GetType() != ParentB.GetType())
            throw new ArgumentException("Cannot breed different species");

        //Don't allow breeeding with self. Maybe in the future asexual reproduction could be implemented?
        if (ParentA == ParentB)
            throw new ArgumentException("Creatures can not breed with themselves");

        Creature newCreature = (Creature)Activator.CreateInstance(ParentA.GetType());

        //Call Breed on every Gene this creature has
        var genes = ParentA.GetType().GetFields()
             .Where(f => f.FieldType.IsSubclassOf(typeof(Gene)));

        foreach(var gene in genes)
        {
            Gene parentAGene = (Gene)gene.GetValue(ParentA);
            Gene parentBGene = (Gene)gene.GetValue(ParentB);
            gene.SetValue(newCreature, Gene.Breed(parentAGene, parentBGene));
        }

        return newCreature;
    }
}
