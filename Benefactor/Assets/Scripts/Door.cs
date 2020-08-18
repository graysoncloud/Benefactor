using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    public bool locked;
    public bool takesKey;
    public DoorSprite doorSprite;

    protected bool open;
    protected InteractableObject trigger;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        doorSprite = Instantiate(doorSprite, transform.position, Quaternion.identity);
    }

    void Open() //thanks to https://answers.unity.com/questions/239912/rotate-door-object-away-from-player.html
    {
        if (open == false)
        { // only open a closed door!
            open = true;
            ErasePosition();
            doorSprite.Open();
        }
    }

    void Close()
    {
        open = false;
        UpdatePosition();
        doorSprite.Close();
    }

    public void Toggle()
    {
        if (open)
            Close();
        else
            Open();
    }

    public bool IsOpen()
    {
        return open;
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (!locked)
            receiveActions.Add("Door");

        return receiveActions;
    }
}
