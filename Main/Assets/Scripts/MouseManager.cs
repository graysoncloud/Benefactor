using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class MouseManager : MonoBehaviour
{
    Vector2 currentMouseCoords;
    MenuManager menuManager;

    void Awake()
    {
        currentMouseCoords = new Vector2{};
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }

    public bool GetNextCharacter(List<Player> characters)
    {
        UpdateMousePosition();
        
        foreach (Player character in characters)
        {
            if ((Vector2)character.transform.position == currentMouseCoords)
            {
                menuManager.HighlightPath(new Vector2[] { currentMouseCoords });

                if (Input.GetMouseButtonDown(0))
                {
                    GameManager.instance.activeCharacter = character;
                    GameManager.instance.CameraTarget(character.gameObject);
                    menuManager.HideIndicators();
                    return false;
                }
            }
            else
                menuManager.UnhighlightPath(new Vector2[] { character.transform.position });
        }

        return true;
    }

    public bool GetMoveInput(Character character, Dictionary<Vector2, Vector2[]> paths)
    {
        UpdateMousePosition();

        if (paths.ContainsKey(currentMouseCoords))
        {
            paths.TryGetValue(currentMouseCoords, out character.pathToObjective);
            menuManager.HighlightPath(character.pathToObjective);

            if (Input.GetMouseButtonDown(0))
            {
                menuManager.HideIndicators();
                return false;
            }
        }
        else
        {
            menuManager.UnhighlightPaths();
        }

        return true;
    }

    public bool GetTargetInput(Character character, List<InteractableObject> objects)
    {
        UpdateMousePosition();
        
        foreach (InteractableObject o in objects)
        {
            if ((Vector2)o.transform.position == currentMouseCoords)
            {
                menuManager.HighlightPath(new Vector2[] { currentMouseCoords });

                if (Input.GetMouseButtonDown(0))
                {
                    character.currentObjective.target = o;
                    GameManager.instance.CameraTarget(o.gameObject);
                    menuManager.HideIndicators();
                    return false;
                }
            }
            else
                menuManager.UnhighlightPath(new Vector2[] { o.transform.position });
        }

        return true;
    }

    public Vector2 UpdateMousePosition()
    {
        Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 coords = new Vector2((int)(mouseWorldPosition.x + 0.5), (int)(mouseWorldPosition.y + 0.5));
        if (currentMouseCoords != coords) {
            menuManager.HideMouseIndicator();
            currentMouseCoords = coords;
            menuManager.ShowMouseIndicator(currentMouseCoords);
        }
        return coords;
    }
}
