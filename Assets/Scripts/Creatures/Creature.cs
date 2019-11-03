using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

public abstract class Creature : MonoBehaviour {

	public static Creature Breed(Creature ParentA, Creature ParentB)
    {
        if (ParentA.GetType() != ParentB.GetType())
            throw new ArgumentException("Cannot breed different species");

        Creature newCreature = (Creature)Activator.CreateInstance(ParentA.GetType());

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
