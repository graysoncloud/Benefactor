using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    public bool locked;
    public bool takesKey;
    public DoorSprite doorSprite;
    public Lever trigger;

    protected bool open;
    protected Animator animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    public void Toggle()
    {
        if (open)
            Close();
        else
            Open();
    }

    void Open()
    {
        open = true;
        animator.SetBool("Open", true);
        ErasePosition();
    }

    void Close()
    {
        open = false;
        animator.SetBool("Open", false);
        UpdatePosition();
    }

    public void Unlock()
    {
        locked = false;
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
        else if (takesKey)
            receiveActions.Add("Unlock");

        return receiveActions;
    }

    protected override void UpdatePosition()
    {
        GameManager.instance.UpdateNode(transform.position, !locked || damageable, -2);
    }

    public void SetupTrigger(Vector2 position)
    {
        trigger = Instantiate(trigger, position, Quaternion.identity);
        trigger.SetTarget(this);
    }

    public Lever GetTrigger()
    {
        return trigger;
    }
}
