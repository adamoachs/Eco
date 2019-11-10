using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimal : Animal
{
    public override float InitialSpeed
    {
        get
        {
            return 1;
        }
    }

    public override float IntitalPopulationDensity
    {
        get
        {
            return 10;
        }
    }
}
