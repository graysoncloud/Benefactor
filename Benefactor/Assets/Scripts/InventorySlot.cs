using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Player player;

    HoldableObject item;

    private void Start()
    {
        //player = GameObject.Find("Player");
    }

    public void AddItem (HoldableObject newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
        button.interactable = true;

        Debug.Log("Added " + newItem.name + " to " + this);
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        button.interactable = false;
    }

    public void OnPress()
    {
        player.ChooseItem(item);
    }
}
