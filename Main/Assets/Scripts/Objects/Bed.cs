using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : InteractableObject
{

    protected override void UpdatePosition()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.y - 1);
        base.UpdatePosition();
        transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.y + 1);
        base.UpdatePosition();
    }
}
