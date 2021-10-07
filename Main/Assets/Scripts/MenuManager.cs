using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class MenuManager : MonoBehaviour
{
    public static void ActionButtonPressed(String action)
    {
        GameManager.instance.activeCharacter.GetActionInput(action);
    }

    public static void BackButtonPressed()
    {
        GameManager.instance.activeCharacter.Back();
    }
}
