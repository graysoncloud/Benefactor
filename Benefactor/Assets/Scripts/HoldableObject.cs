using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    public string type;
    public int uses; //# of uses before breaking or amount of ammo for guns
    public int amount; //amount of health to heal or to damage
    public int range; //distance to shoot or throw an object
    public int blastRadius; //blast radius for area of effect throwables

    // Start is called before the first frame update
    void Start()
    {
        type = "Food";
    }
}
