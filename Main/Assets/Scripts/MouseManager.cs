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
    public bool GetNextCharacter(List<Player> characters)
    {
        Vector2 coords = GetMousePosition();
        
        foreach (Player character in characters)
        {
            if ((Vector2)character.transform.position == coords)
            {
                GameObject.Find("MenuManager").GetComponent<MenuManager>().HighlightPath(new Vector2[] { coords });

                if (Input.GetMouseButtonDown(0))
                {
                    GameManager.instance.activeCharacter = character;
                    GameManager.instance.CameraTarget(character.gameObject);
                    GameObject.Find("MenuManager").GetComponent<MenuManager>().HideIndicators();
                    return false;
                }
            }
            else
                GameObject.Find("MenuManager").GetComponent<MenuManager>().UnhighlightPath(new Vector2[] { character.transform.position });
        }

        return true;
    }

    public bool GetMoveInput(Character character, Dictionary<Vector2, Vector2[]> paths)
    {
        Vector2 coords = GetMousePosition();

        if (paths.ContainsKey(coords))
        {
            paths.TryGetValue(coords, out character.pathToObjective);
            GameObject.Find("MenuManager").GetComponent<MenuManager>().HighlightPath(character.pathToObjective);

            if (Input.GetMouseButtonDown(0))
            {
                GameObject.Find("MenuManager").GetComponent<MenuManager>().HideIndicators();
                return false;
            }
        }
        else
        {
            GameObject.Find("MenuManager").GetComponent<MenuManager>().UnhighlightPaths();
        }

        return true;
    }

    public bool GetTargetInput(Character character, List<InteractableObject> objects)
    {
        Vector2 coords = GetMousePosition();
        
        foreach (InteractableObject o in objects)
        {
            if ((Vector2)o.transform.position == coords)
            {
                GameObject.Find("MenuManager").GetComponent<MenuManager>().HighlightPath(new Vector2[] { coords });

                if (Input.GetMouseButtonDown(0))
                {
                    character.currentObjective.target = o;
                    GameManager.instance.CameraTarget(o.gameObject);
                    GameObject.Find("MenuManager").GetComponent<MenuManager>().HideIndicators();
                    return false;
                }
            }
            else
                GameObject.Find("MenuManager").GetComponent<MenuManager>().UnhighlightPath(new Vector2[] { o.transform.position });
        }

        return true;
    }

    private Vector2 GetMousePosition()
    {
        Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 coords = new Vector2((int)(mouseWorldPosition.x + 0.5), (int)(mouseWorldPosition.y + 0.5));
        return coords;
    }
}
