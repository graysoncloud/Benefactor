using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : InteractableObject
{
    public float offset;

    // Start is called before the first frame update
    protected override void UpdatePosition()
    {
        GameManager.instance.UpdateNode(transform.position, true, 0);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + offset);
    }
}
