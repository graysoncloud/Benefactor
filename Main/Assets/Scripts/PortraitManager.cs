using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitManager : MonoBehaviour
{
    Image ImageComponent;

    public Sprite BlankPortrait;
    public Sprite RaskolnikovNeutral;
    public Sprite IvanovnaNeutral;
    public Sprite Pepov;
    public Sprite Patron;
    public Sprite Sergey;

    public void Awake()
    {
        ImageComponent = GetComponent<Image>();
    }

    public void changePortrait(string newPortraitName)
    {
        switch (newPortraitName)
        {
            case "RaskolnikovNeutral": 
                ImageComponent.sprite = RaskolnikovNeutral;
                break;
            case "IvanovnaNeutral":
                ImageComponent.sprite = IvanovnaNeutral;
                break;
            case "BlankPortrait":
                ImageComponent.sprite = BlankPortrait;
                break;
            case "Pepov":
                ImageComponent.sprite = Pepov;
                break;
            case "Patron":
                ImageComponent.sprite = Patron;
                break;
            case "Sergey":
                ImageComponent.sprite = Sergey;
                break;
            default:
                Debug.LogError("Invalid portrait name: " + newPortraitName);
                break;

        }
    }

}
