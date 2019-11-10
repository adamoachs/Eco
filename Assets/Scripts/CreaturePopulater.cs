using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;

public class CreaturePopulater
{
    private TerrainGenerator terrainGenerator;

    //Block instantiation without world size
    private CreaturePopulater() { }

    //Returns all Prefabs in the "Resources/Prefabs/Creatures" folder that contain a Script component inheriting Creature
    private List<GameObject> creaturePrefabs
    {
        get
        {
            var prefabs = Resources.LoadAll("Prefabs/Creatures").Cast<GameObject>();
            return prefabs.Where(p => p.GetComponents<Creature>().FirstOrDefault() as Creature != null).ToList();
        }
    }

    public CreaturePopulater(TerrainGenerator terrainGenerator)
    {
        this.terrainGenerator = terrainGenerator;
    }

    public void Populate()
    {
        //Loop through all our defined Creature prefabs, as well as get their attached script
        foreach(var creaturePrefab in creaturePrefabs)
        {
            Creature creatureScript = creaturePrefab.GetComponent<Creature>() as Creature;
            int populationNeeded = Convert.ToInt32(terrainGenerator.WorldSizeX * terrainGenerator.WorldSizeZ / 1000 * creatureScript.IntitalPopulationDensity);
            int populationCreated = 0;
            while(populationCreated < populationNeeded)
            {
                float x = UnityEngine.Random.Range(0, terrainGenerator.WorldSizeX);
                float z = UnityEngine.Random.Range(0, terrainGenerator.WorldSizeZ);
                GameObject tile = terrainGenerator.GetTileAtPosition(x, z);

                //Prohibit spawning on water
                if(tile.name.Contains("Cube"))
                {
                    GameObject creature = GameObject.Instantiate(creaturePrefab, new Vector3(x, tile.transform.position.y + 1, z), Quaternion.identity);
                    creatureScript = creature.GetComponent<Creature>() as Creature;
                    creatureScript.SetDefaults();
                    populationCreated++;
                }
                
            }
        }
        
    }
}
