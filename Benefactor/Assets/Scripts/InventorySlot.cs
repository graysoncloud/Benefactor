using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    HoldableObject item;

    public void AddItem (HoldableObject newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;

        Debug.Log("Added " + newItem.name + " to " + this);
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
    }
}
