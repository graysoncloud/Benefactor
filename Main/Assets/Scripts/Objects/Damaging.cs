using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaging : MonoBehaviour
{
    protected BoxCollider2D boxCollider;
    public int cost;
    public int damagePerTurn;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        GameManager.instance.UpdateNode(transform.position, true, cost); //set however much it should "cost" to take dmg vs going around
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
