using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    //Public properties; set these in inspecter
    public int WorldSizeX;
    public int WorldSizeZ;

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
    private float calculatedMedianHeight;

    void Start()
    {
        vectors = new Vector3[WorldSizeX * WorldSizeZ];
        seed_x = Random.Range(-10000, 10000);
        seed_z = Random.Range(-10000, 10000);
        offset_seed_x = Random.Range(-10000, 10000);
        offset_seed_z = Random.Range(-10000, 10000);
        //seed_x = 5052;
        //seed_z = -2826;
        //offset_seed_x = 123;
        //offset_seed_z = 785;

        GenerateLand();
        GenerateWater();
    }

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

                float y = Perlin(xf, zf, 40, 1f)
                        + Perlin(xf, zf, 3, 0.1f);

                //Store them rather than generating terrain immediately, because there is additional processing to do on it first.
                vectors[numVectorsAdded] = new Vector3(x, y, z);
                numVectorsAdded++;
            }
        }

        //Calculations for other stuff prior to building the cubes
        CorrectWaterCoverage();
        calculatedMedianHeight = Utility.Percentile(vectors.Select(v => v.y), 0.5F);

        //Loop through array of vectors and instantiate cubes
        foreach (Vector3 vector in vectors)
        {
            var cube = Instantiate(GetTileTypeFromLandHeight(vector), vector, Quaternion.identity);
            cube.transform.parent = this.transform;
        }
    }

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

        //If we adjust up/down, just to realize we adjusted too far up/down and have to go back down/back up, then decrease the amount we adjust by
        if (previousDirection.HasValue && previousDirection != direction)
            numFlips++;
        float correctionFactor = direction * 2 * Mathf.Pow(4, -1 * numFlips);

        //Adjust all the Y coordinates
        for(int i = 0; i < vectors.Length; i++)
        {
            vectors[i] += new Vector3(0, correctionFactor, 0);
        }

        //Recurse to check the new result
        CorrectWaterCoverage(direction, numFlips);
    }

    private GameObject GetTileTypeFromLandHeight(Vector3 pos)
    {
        //Add a perlin noise based offset to the y position we use to calculate tile type
        float amplitude = 6F;
        float offset = (Perlin((pos.x + offset_seed_x)/WorldSizeX, (pos.z + offset_seed_z)/WorldSizeZ, amplitude, 0.1F) - (0.5F * amplitude));

        //Scale the offset according to how close to the median height. This gives us more variation between grass/stone at the peaks, and less variation between sand/dirt at the beaches
        offset *= Mathf.Pow(pos.y / calculatedMedianHeight, 2);

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

    //Old method to generate the landscape as a single mesh. Decided I like the look of a bunch of cubes instead
    /*
    private void GenerateLand()
    {
        //prepare mesh objects
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();
        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        float seedx = Random.Range(-10000, 10000);
        float seedz = Random.Range(-10000, 10000);
        List<Vector3> lstVertices = new List<Vector3>();
        Dictionary<Tuple<int, int>, float> dictVerts = new Dictionary<Tuple<int, int>, float>();
        List<int> tris = new List<int>();

        //Generate vertices, iterating through x and z points, 
        //mapping them to a dictionary of y points generated from Perlin noise
        for (int x = 0; x < WorldSizeX; x++)
        {
            for (int z = 0; z < WorldSizeZ; z++)
            {
                float xf = (x + seedx) / WorldSizeX;
                float zf = (z + seedz) / WorldSizeZ;

                float y = Perlin(xf, zf, 40, 1f)
                        + Perlin(xf, zf, 3, 0.1f);

                dictVerts.Add(new Tuple<int, int>(x, z), y);
            }
        }

        //Generate vertices and triangles from vertices
        int vertCount = 0;
        for (int x = 0; x < WorldSizeX - 1; x++)
        {
            for (int z = 0; z < WorldSizeZ - 1; z++)
            {
                float ll_y = dictVerts[new Tuple<int, int>(x, z)];
                lstVertices.Add(new Vector3(x, ll_y, z));
                int ll_i = vertCount++;

                float lr_y = dictVerts[new Tuple<int, int>(x + 1, z)];
                lstVertices.Add(new Vector3(x + 1, lr_y, z));
                int lr_i = vertCount++;

                float ul_y = dictVerts[new Tuple<int, int>(x, z + 1)];
                lstVertices.Add(new Vector3(x, ul_y, z + 1));
                int ul_i = vertCount++;

                float ur_y = dictVerts[new Tuple<int, int>(x + 1, z + 1)];
                lstVertices.Add(new Vector3(x + 1, ur_y, z + 1));
                int ur_i = vertCount++;

                tris.AddRange(new int[3] { ll_i, ul_i, ur_i });
                tris.AddRange(new int[3] { ll_i, ur_i, lr_i });
            }
        }

        //Finish mesh properties
        mesh.vertices = lstVertices.ToArray();
        mesh.triangles = tris.ToArray();

        //TODO: Generate texture based on height/proximity to water
        meshRenderer.material = Resources.Load("Materials/Grass") as Material;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
    */

    private float Perlin(float x, float z, float amplitude, float frequency)
    {
        return amplitude * Mathf.PerlinNoise(x / frequency, z / frequency);
    }
}
