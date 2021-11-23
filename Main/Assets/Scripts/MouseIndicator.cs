using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class MouseIndicator : MonoBehaviour
{
    public InteractableObject FindInteractableObject()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll((Vector2)transform.position, 0.1f);
        foreach (Collider2D hitCollider in hitColliders) {
            InteractableObject interactableObject = hitCollider.GetComponent<InteractableObject>();
            if (interactableObject != null)
            {
                return interactableObject;
            }
        }
        return null;

        // try
        // {
        //     Roof roof = hitCollider.GetComponent<Roof>();
        //     if (roof != null && roof.Hidden())
        //     {
        //         roof.GetComponent<BoxCollider2D>().enabled = false;
        //         hitCollider = FindInteractableObject().GetComponent<Collider2D>();
        //         roof.GetComponent<BoxCollider2D>().enabled = true;
        //     }
        // }
        // catch {}

        // try
        // {
        //     Leaves leaves = hitCollider.GetComponent<Leaves>();
        //     if (leaves != null)
        //     {
        //         leaves.GetComponent<BoxCollider2D>().enabled = false;
        //         hitCollider = FindInteractableObject().GetComponent<Collider2D>();
        //         leaves.GetComponent<BoxCollider2D>().enabled = true;
        //     }
        // }
        // catch {}
        // return hitCollider != null ? hitCollider.GetComponent<InteractableObject>() : null;
    }
}