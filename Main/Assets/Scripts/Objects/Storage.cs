using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : InteractableObject
{
    public bool locked;
    public bool takesKey;
    public List<HoldableObject> items;
    public Sprite openSprite;
    public Sprite closedSprite;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public void Open()
    {
        spriteRenderer.sprite = openSprite;
    }

    public void Close()
    {
        spriteRenderer.sprite = closedSprite;
    }

    public void Unlock()
    {
        locked = false;
        spriteRenderer.sprite = closedSprite;
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (!locked)
            receiveActions.Add("Loot");
        else if (takesKey)
            receiveActions.Add("Unlock");

        return receiveActions;
    }

    public void Remove(HoldableObject item)
    {
        items.Remove(item);
    }

}
