﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;

public class Player : Character
{
    public Text rationaleText;
    public Text healthText;

    public GameObject tileIndicator;
    public List<GameObject> indicators;
    public CanvasGroup actionMenu;
    private List<GameObject> actionButtons;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        maxHealth = 10;
        health = maxHealth;
        rationaleText = GameObject.Find("RationaleText").GetComponent<Text>();
        rationaleText.text = "Rationale: " + rationale;
        healthText = GameObject.Find("HealthText").GetComponent<Text>();
        healthText.text = "Health: " + health;
        actionMenu = GameObject.Find("ActionPanel").GetComponent<CanvasGroup>();
        actionMenu.alpha = 0f;
        actionMenu.blocksRaycasts = false;
        actionButtons = new List<GameObject>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (gettingMove)
        {
            gettingMove = GetMoveInput();
            if (!gettingMove)
            {
                StartCoroutine(FollowPath());
            }
        }
        if (gettingTarget)
        {
            gettingTarget = GetTargetInput();
            if (!gettingTarget)
            {
                if (target == this)
                    GetActionInput("");
                else if (target.receiveActions.Count > 1)
                {
                    SetupActionMenu();
                }
                else
                    GetActionInput(target.receiveActions[0]);
            }
        }
    }

    protected override void GetMove()
    {
        ShowPaths();
        Debug.Log("Player waiting for move input");
    }

    bool GetMoveInput()
    {
        int tileWidth = 56; //Don't know actual tile size yet! This is what I guessed
        Vector2 camera = Camera.main.transform.position;
        int x = (int)((Input.mousePosition.x - Screen.width / 2 - tileWidth / 2) / tileWidth + camera.x + 1);
        int y = (int)((Input.mousePosition.y - Screen.height / 2 - tileWidth / 2) / tileWidth + camera.y + 1);
        Vector2 coords = new Vector2(x, y);

        if (paths.ContainsKey(coords))
        {
            Vector2[] path;
            paths.TryGetValue(coords, out path);
            HighlightPath(path);

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(camera);
                toMove = coords;
                HideIndicators();
                return false;
            }
        }
        else
        {
            UnhighlightPath();
        }

        return true;
    }

    protected override void GetTarget()
    {
        GetNearbyObjects();
        if (nearbyObjects.Count == 1)
            EndTurn();
        else
        {
            gettingTarget = true;
            ShowNearbyObjects();
            Debug.Log("Player waiting for target input");
        }
    }

    bool GetTargetInput()
    {
        int tileWidth = 56; //Don't know actual tile size yet! This is what I guessed
        Vector2 camera = Camera.main.transform.position;
        int x = (int)((Input.mousePosition.x - Screen.width / 2 - tileWidth / 2) / tileWidth + camera.x + 1);
        int y = (int)((Input.mousePosition.y - Screen.height / 2 - tileWidth / 2) / tileWidth + camera.y + 1);
        Vector2 coords = new Vector2(x, y);
        Dictionary<Vector2, InteractableObject> targets = new Dictionary<Vector2, InteractableObject>();
        foreach (InteractableObject nearbyObject in nearbyObjects)
        {
            targets.Add(nearbyObject.transform.position, nearbyObject);
        }
        if (targets.ContainsKey(coords))
        {
            HighlightPath(new Vector2[]{ coords });

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(camera);
                targets.TryGetValue(coords, out target);
                HideIndicators();
                return false;
            }
        }
        else
        {
            UnhighlightPath();
        }

        return true;
    }

    void SetupActionMenu()
    {
        int index = 0,
            buttonHeight = 30,
            buttonWidth = 160,
            startPosition = buttonHeight * (int)target.receiveActions.Count / 2;
        RectTransform panelRectTransform = GameObject.Find("ActionPanel").transform.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(buttonWidth + 10, (buttonHeight+10) * target.receiveActions.Count); //panelRectTransform.sizeDelta.y
        foreach (string action in receiveActions)
        {
            GameObject button = GameObject.Find($"{action}Button");
            //button.SetActive(true);
            actionButtons.Add(button);
            button.transform.position = new Vector2(Screen.width / 2, Screen.height / 2 + startPosition - 5 - (buttonHeight+10)*index);
            index++;
        }

        actionMenu.alpha = 1f;
        actionMenu.blocksRaycasts = true;
        Debug.Log("Player waiting for act input");
    }

    void HideActionMenu()
    {
        actionMenu.alpha = 0f;
        actionMenu.blocksRaycasts = false;
        //foreach (GameObject button in actionButtons)
        //{
        //    button.SetActive(false);
        //}
        actionButtons.Clear();
    }

    public void GetActionInput(string action)
    {
        HideActionMenu();

        targetAction = action;
        Act();
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            rationale += 3;
            Invoke("Restart", 1f);
            enabled = false;
        }

        base.OnTriggerEnter2D(other);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void TakeDamage (double loss)
    {
        base.TakeDamage(loss);
        healthText.text = "Health: " + health;
        animator.SetTrigger("playerHit");
        CheckIfGameOver();
    }

    public override void Heal (int amount)
    {
        base.Heal(amount);
        healthText.text = "Health: " + health;
    }

    private void CheckIfGameOver ()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }

    private void ShowPaths()
    {
        HideIndicators();

        foreach (KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            indicators.Add(Instantiate(tileIndicator, entry.Key, Quaternion.identity));
            indicators[indicators.Count - 1].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    public void ShowNearbyObjects()
    {
        HideIndicators();

        foreach (InteractableObject nearbyObject in nearbyObjects)
        {
            indicators.Add(Instantiate(tileIndicator, nearbyObject.transform.position, Quaternion.identity));
            indicators[indicators.Count - 1].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    private void HideIndicators()
    {
        foreach (GameObject indicator in indicators)
        {
            Destroy(indicator);
        }
        indicators.Clear();
    }

    private void HighlightPath(Vector2[] path)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            if (path.Contains((Vector2)indicator.transform.position))
            {
                indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
    }

    private void UnhighlightPath()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }
}
