using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : InteractableObject
{
    public Sprite onSprite;
    public Sprite offSprite;

    protected bool on;
    protected Door target;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        on = true;
    }

    public void SetTarget(Door target)
    {
        this.target = target;
    }

    public void Toggle()
    {
        target.Toggle();
        on = !on;
        UpdateSprite();
    }

    public bool IsOn()
    {
        return on;
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        receiveActions.Add("Lever");

        return receiveActions;
    }

    private void UpdateSprite()
    {
        spriteRenderer.sprite = on ? onSprite : offSprite;
    }
}
