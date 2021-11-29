using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : InteractableObject
{

    protected override void UpdatePosition()
    {
        GameManager.instance.UpdateNode(transform.position - new Vector3(0,1,1), damageable, walkOver ? 0 : (float)health);
        base.UpdatePosition();
    }
}
