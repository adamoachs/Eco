using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// An abstract base class that every animal in Eco derives from
/// </summary>
public abstract class Animal : Creature
{
    /// <summary>
    /// Speed defines how many times per second to move
    /// </summary>
    public abstract float InitialSpeed { get; }

    protected SpeedGene SpeedGene { get; set; }
    protected TerrainGenerator terrainGenerator;

    private CharacterController controller;
    private Vector3 destination;

    public void Start()
    {
        terrainGenerator = GameObject.Find("TerrainHandler").GetComponent("TerrainGenerator") as TerrainGenerator;
        controller = GetComponent<CharacterController>();
        InvokeRepeating("OnTick", UnityEngine.Random.Range(0F,1F), 1 / SpeedGene.Value);
    }

    private void OnTick()
    {
        List<Vector3> vectors = new List<Vector3>()
        {
            Vector3.left,
            Vector3.right,
            Vector3.back,
            Vector3.forward
        };
        Vector3 destVector = vectors[UnityEngine.Random.Range(0,vectors.Count)];
        destination = new Vector3((float)Math.Round(transform.position.x), transform.position.y + 1, (float)Math.Round(transform.position.z)) + destVector;
        destination = new Vector3(destination.x, terrainGenerator.GetY(destination.x, destination.z), destination.z);
        transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z));
    }

    private void Update()
    {
        //Movement
        var offset = destination - transform.position;
        if(offset.magnitude > 0.1f)
        {
            controller.SimpleMove(offset.normalized * Time.deltaTime * 60 * 5);
        }
    }

    public override void SetDefaults()
    {
        this.SpeedGene = new SpeedGene()
        {
            Value = InitialSpeed
        };
    }

}
