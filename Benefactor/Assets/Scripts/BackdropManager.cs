using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackdropManager : MonoBehaviour
{
    Image ImageComponent;

    public Sprite Thoroughfair;
    public Sprite InterogationOffice;

    public void Start()
    {
        ImageComponent = GetComponent<Image>();
    }

    public void changeBackdrop(string newBackdropName)
    {
        switch (newBackdropName)
        {
            case "Thoroughfair":
                ImageComponent.sprite = Thoroughfair;
                break;
            case "InterogationOffice":
                ImageComponent.sprite = InterogationOffice;
                break;
            default:
                Debug.LogError("Invalid portrait name: " + newBackdropName);
                break;
        }
    }

}
