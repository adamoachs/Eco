using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class implemented a perlin noise based procedural terrain generation system
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    //Public properties; set these in inspecter
    public int WorldSizeX;
    public int WorldSizeZ;

    //Prefabs for the land and water tiles
    public GameObject deepWaterPrefab;
    public GameObject medWaterPrefab;
    public GameObject shallowWaterPrefab;

    public GameObject stonePrefab;
    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject sandPrefab;

    //Generation settings; change these to affect generation
    private const float DIRT_START_HEIGHT = 11;
    private const float GRASS_START_HEIGHT = 12;
    private const float STONE_START_HEIGHT = 20;
    private const float LAYER_HEIGHT_RANDOMNESS = 0.5F;
    private const float WATER_HEIGHT = 10;
    private const float MED_WATER_DEPTH = 2;
    private const float DEEP_WATER_DEPTH = 4;
    private const float MAX_WATER_COVERAGE = 0.35F;
    private const float MIN_WATER_COVERAGE = 0.20F;

    //Calculated fields
    private float seed_x;
    private float seed_z;
    private float offset_seed_x;
    private float offset_seed_z;
    private Vector3[] vectors;
    private float medianHeight;

    //Init
    void Start()
    {
        vectors = new Vector3[WorldSizeX * WorldSizeZ];
        seed_x = Random.Range(-10000, 10000);
        seed_z = Random.Range(-10000, 10000);
        offset_seed_x = Random.Range(-10000, 10000);
        offset_seed_z = Random.Range(-10000, 10000);

        GenerateLand();
        GenerateWater();
    }

    /// <summary>
    /// Generates cubes for land
    /// </summary>
    private void GenerateLand()
    {
        //Iterate through all X and Z coordinates to generate Y coorindates based on perlin noise.
        int numVectorsAdded = 0;
        for (int x = 0; x < WorldSizeX; x++)
        {
            for (int z = 0; z < WorldSizeZ; z++)
            {
                float xf = (x + seed_x) / WorldSizeX;
                float zf = (z + seed_z) / WorldSizeZ;

                //Call the Perlin function a few times, with differeing amplitudes and frequencies
                float y = Perlin(xf, zf, 40, 1f)
                        + Perlin(xf, zf, 3, 0.1f);

                //Store them rather than generating terrain immediately, because there is additional processing to do on it first.
                vectors[numVectorsAdded] = new Vector3(x, y, z);
                numVectorsAdded++;
            }
        }

        //Calculations for other stuff prior to building the cubes
        CorrectWaterCoverage();
        medianHeight = Utility.Percentile(vectors.Select(v => v.y), 0.5F);

        //Loop through array of vectors and instantiate cubes
        foreach (Vector3 vector in vectors)
        {
            var cube = Instantiate(GetTileTypeFromLandHeight(vector), vector, Quaternion.identity);
            cube.transform.parent = this.transform;
        }
    }

    /// <summary>
    /// Generates water tiles above the land, where the land is below the water level
    /// </summary>
    private void GenerateWater()
    {
        foreach (Vector3 landVector in vectors)
        {
            GameObject waterType = GetWaterTypeFromDepth(landVector);

            if (waterType != null)
            {
                var water = Instantiate(waterType, new Vector3(landVector.x, WATER_HEIGHT, landVector.z), Quaternion.identity);
                water.transform.parent = this.transform;
            }
        }
    }

    /// <summary>
    /// Ensure we have a good percent of water covereage; no desert worlds of water worlds
    /// </summary>
    /// <param name="previousDirection">Only for recursive calls, use default value</param>
    /// <param name="numFlips">Only for recursive calls, use default value</param>
    private void CorrectWaterCoverage(float? previousDirection = null, float numFlips = 1)
    {
        //Calculate percent water coverage as the percent of vectors with a y coordinate below the WATER_HEIGHT
        float waterCoverage = ((float)vectors.Count(v => v.y < WATER_HEIGHT)) / vectors.Length;

        //Are we good?
        if(waterCoverage > MIN_WATER_COVERAGE && waterCoverage < MAX_WATER_COVERAGE)
        {
            return;
        }

        //Determine if we need to move terrain up or down
        float direction = waterCoverage < MIN_WATER_COVERAGE ? -1 : 1;

        //We don't want to adjust up/down, just to realize we adjusted too far up/down and have to go back down/back up
        //If we have to flip the direction, then decrease the amount we adjust by
        //Ideally, we could somehow calculate how much to raise/lower the terrain in one go, but that seems like a complicated calculation
        if (previousDirection.HasValue && previousDirection != direction)
            numFlips++;
        float correctionFactor = direction * 2 * Mathf.Pow(4, -1 * numFlips);

        //Adjust all the Y coordinates
        for(int i = 0; i < vectors.Length; i++)
        {
            //We adjust the height of the terrain rather than the water itself, in order to keep the water level at the WATER_LEVEL const
            vectors[i] += new Vector3(0, correctionFactor, 0);
        }

        //Recurse to check the new result
        CorrectWaterCoverage(direction, numFlips);
    }

    /// <summary>
    /// What type of tile we use for land is based on the Y coordinate, with sand at the lowest level (beaches) and stone at the highest (mountains)
    /// </summary>
    /// <param name="pos">Position of tile to place</param>
    /// <returns>GameObject of the prefab to place</returns>
    private GameObject GetTileTypeFromLandHeight(Vector3 pos)
    {
        //Add a perlin noise based offset to the y position we use to calculate tile type
        //This adds some variation, like grassy paths through rocky mountains
        float amplitude = 6F;
        float offset = (Perlin((pos.x + offset_seed_x)/WorldSizeX, (pos.z + offset_seed_z)/WorldSizeZ, amplitude, 0.1F) - (0.5F * amplitude));

        //Scale the offset according to height, to give more variation between grass/stone at the peaks, and less variation between sand/dirt at the beaches
        offset *= Mathf.Pow(pos.y / medianHeight, 2);

        float y = pos.y + offset;
        if (y > STONE_START_HEIGHT)
            return stonePrefab;
        else if (y > GRASS_START_HEIGHT)
            return grassPrefab;
        else if (y > DIRT_START_HEIGHT)
            return dirtPrefab;
        else
            return sandPrefab;
    }

    /// <summary>
    /// What type of tile we use for water is based on the Y coordinate of the land it's above. The deeper it is, the darker water tile we use
    /// </summary>
    /// <param name="pos">Position of tile to place</param>
    /// <returns>GameObject of the prefab to place</returns>
    private GameObject GetWaterTypeFromDepth(Vector3 pos)
    {
        float depth = WATER_HEIGHT - pos.y;

        if (depth <= 0)
            return null;
        else if (depth > DEEP_WATER_DEPTH)
            return deepWaterPrefab;
        else if (depth > MED_WATER_DEPTH)
            return medWaterPrefab;
        else
            return shallowWaterPrefab;
    }

    /// <summary>
    /// Encapsulate an Amplitude and Frequency argument into the existing Mathf.PerlinNoise method
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="z">Y coordinate</param>
    /// <param name="amplitude">Amplitude of the perlin output</param>
    /// <param name="frequency">Frequency of the perlin output</param>
    /// <returns>Y coordinate corresponding to the given X+Z input</returns>
    private float Perlin(float x, float z, float amplitude, float frequency)
    {
        return amplitude * Mathf.PerlinNoise(x / frequency, z / frequency);
    }
}
