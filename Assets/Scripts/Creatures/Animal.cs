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
    private const int MOVEMENT_DECISIVENESS = 10;
    /// <summary>
    /// Speed defines how many times per second to move
    /// </summary>
    public abstract float InitialSpeed { get; }

    protected SpeedGene SpeedGene { get; set; }
    protected TerrainGenerator terrainGenerator;

    private CharacterController myController;
    private Collider myCollider;

    //Position to move to across several steps
    private Vector3 destination;

    //Absolute coordinates of the next step we're moving to on the way to destination
    private Vector3 destination_nextStep;

    public void Start()
    {
        terrainGenerator = GameObject.Find("TerrainHandler").GetComponent("TerrainGenerator") as TerrainGenerator;
        myController = GetComponent<CharacterController>();
        myCollider = GetComponent<Collider>();
        destination = GetNewDestination();
        InvokeRepeating("MoveTick", UnityEngine.Random.Range(0F,1F), 1 / SpeedGene.Value);
    }

    private void MoveTick()
    {
        //If we're at our destination, or sometimes randomly, choose a new direction to wander towards
        if(Vector3.Distance(destination, transform.position) < 0.1 || UnityEngine.Random.Range(1,MOVEMENT_DECISIVENESS) == 1) 
        {
            destination = GetNewDestination();
        }

        //Choose the next step in our grid to wander to
        //Move a distance of one along a vector between us and the destination
        Vector3 directionToDestination = destination - transform.position;
        directionToDestination.y = 0;
        Vector3 newStep = transform.position + directionToDestination.normalized;

        //Get the tile at that position and move towards it
        GameObject destinationTile = terrainGenerator.GetTileAtPosition(newStep.x, newStep.z);
        newStep = destinationTile.transform.position;
        
        if (IsPositionAvailable(newStep))
        {
            destination_nextStep = newStep;
            transform.LookAt(new Vector3(destination_nextStep.x, transform.position.y, destination_nextStep.z));
        }
        else
        {
            //Maybe something here in the future; for now just stay where it is
        }
    }

    private Vector3 GetNewDestination()
    {
        //TODO: Don't try to run straight into a wall
        
        //Choose a random position to move to
        float r = MOVEMENT_DECISIVENESS;
        float theta = UnityEngine.Random.Range(0F, 2F * Mathf.PI);
        return transform.position + new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
    }
    
    private void Update()
    {
        //Movement
        var offset = destination_nextStep - transform.position;
        if(offset.magnitude > 0.1f)
        {
            myController.SimpleMove(offset.normalized * Time.deltaTime * 60 * 5);
        }
    }

    private bool IsPositionAvailable(Vector3 pos)
    {
        var colliders = Physics.OverlapSphere(pos, 2F).Where(c => c != myCollider);
        var gameObjectsHit = colliders.Select(c => c.gameObject);
        foreach(var gameObject in gameObjectsHit)
        {
            Animal a = gameObject.GetComponent("Animal") as Animal;
            //Continue if object found isn't an animal
            if (a == null)
                continue;

            //Return false if position is already occupied
            if (Vector3.Distance(pos, gameObject.transform.position) < 0.1)
                return false;
            
            //Return false if position has an animal already moving towards it
            if (a != null && Vector3.Distance(pos, a.destination_nextStep) < 0.1)
                return false;
        }

        return true;
    }

    public override void SetDefaults()
    {
        this.SpeedGene = new SpeedGene()
        {
            Value = InitialSpeed
        };
    }

}
