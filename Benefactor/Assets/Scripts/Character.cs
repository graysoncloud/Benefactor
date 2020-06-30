using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class NewBehaviourScript : InteractableObject
{
    public int moves;
    public int reputation;

    // Start is called before the first frame update
    void Start()
    {

        base.Start();
    }

    /**
     * Ideally, MoveTo will utilize a SingleMove() function to provide smooth movement one tile at a time, but this will function for the MVP
     * 
     * @param toMoveTo The intended destination for the object
     */
    public void MoveTo(Vector3 toMoveTo)
    {
        transform.position = toMoveTo;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
