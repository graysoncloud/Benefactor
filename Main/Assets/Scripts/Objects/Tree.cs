using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : InteractableObject
{
    public Leaves leaves;
    public Sprite overview;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        leaves = Instantiate(leaves, transform.position, Quaternion.identity);
    }

    protected override void ErasePosition()
    {
        GameObject.Destroy(leaves.gameObject);
        base.ErasePosition();
    }
}
