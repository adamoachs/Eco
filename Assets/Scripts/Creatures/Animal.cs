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
    public SpeedGene Speed { get; private set; }

    private CharacterController controller;
    private Vector3 destination;

    public Animal()
    {
        Speed = new SpeedGene()
        {
            Value = 1
        };
    }

    public void Start()
    {
        controller = GetComponent<CharacterController>();
        InvokeRepeating("OnTick", 1 / Speed.Value, 1 / Speed.Value);
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
        destination = new Vector3((float)Math.Round(transform.position.x), transform.position.y, (float)Math.Round(transform.position.z)) + destVector;
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
}
