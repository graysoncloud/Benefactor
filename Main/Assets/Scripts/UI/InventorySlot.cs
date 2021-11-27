using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Text text;

    HoldableObject item;

    public void AddItem (HoldableObject newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
        text.text = "x" + newItem.uses;
        button.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        text.text = "";
        button.interactable = false;
    }

    public void OnPress()
    {
        GameManager.instance.activeCharacter.ChooseItem(item);
    }
}
