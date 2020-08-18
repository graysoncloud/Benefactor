using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.SocialPlatforms;

public class Player : Character
{
    public Transform itemsParent;
    public GameObject inventoryUI;
    public InventorySlot[] slots;
    public Text rationaleText;
    public Text healthText;
    public bool gettingMove;
    public bool gettingTarget;

    public GameObject tileIndicator;
    public List<GameObject> indicators;
    private CanvasGroup actionMenu;
    private Dictionary<String, GameObject> actionButtons;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        gettingMove = false;
        gettingTarget = false;

        rationaleText = GameObject.Find("RationaleText").GetComponent<Text>();
        rationaleText.text = "Rationale: " + rationale;
        healthText = GameObject.Find("HealthText").GetComponent<Text>();
        healthText.text = "Health: " + health;
        
        actionMenu = GameObject.Find("ActionPanel").GetComponent<CanvasGroup>();
        actionButtons = new Dictionary<String, GameObject>();
        actionButtons.Add("Attack", GameObject.Find("AttackButton"));
        actionButtons.Add("Talk", GameObject.Find("TalkButton"));
        actionButtons.Add("Heal", GameObject.Find("HealButton"));
        actionButtons.Add("Door", GameObject.Find("DoorButton"));
        actionButtons.Add("Unlock", GameObject.Find("UnlockButton"));
        actionButtons.Add("Wait", GameObject.Find("WaitButton"));
        HideActionMenu();

        inventoryUI = GameObject.Find("InventoryParent");
        itemsParent = GameObject.Find("Inventory").GetComponent<Transform>();
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        HideInventory();
    }

    // Update is called once per frame
    void Update()
    {
        if (gettingMove)
        {
            gettingMove = GetMoveInput();
            if (!gettingMove)
            {
                StartCoroutine(SelectedPath());
            }
        }
        if (gettingTarget)
        {
            gettingTarget = GetTargetInput();
            if (!gettingTarget)
            {
                Act();
            }
        }
    }

    public override IEnumerator StartTurn()
    {
        isTurn = true;
        UpdateObjectives();
        GetPaths();
        yield return new WaitForSeconds(moveTime);
        FindPath();
    }

    protected void GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], moves);
    }

    protected void GetPaths(Vector2 next, Vector2[] path, int remainingMoves) //update with better alg/queue?
    {
        if (Array.Exists(path, element => element == next)) { return; }
        Vector2 previous = ((path.Length == 0) ? (Vector2)transform.position : path[path.Length - 1]);
        boxCollider.enabled = false;
        RaycastHit2D[] hits = Physics2D.LinecastAll(previous, next, Collisions);
        boxCollider.enabled = true;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
            {
                Door door = hit.collider.GetComponent<Door>();
                if (door == null || !door.IsOpen())
                    return;
            }
        }

        Vector2[] newPath = new Vector2[path.Length + 1];
        Array.Copy(path, newPath, path.Length);
        newPath[newPath.Length - 1] = next;
        if (!paths.ContainsKey(next)) { paths.Add(next, newPath); }
        else
        {
            paths.TryGetValue(next, out path);
            if (newPath.Length < path.Length)
            {
                paths.Remove(next);
                paths.Add(next, newPath);
            }
            else
            {
                return;
            }
        }

        remainingMoves--;
        if (remainingMoves >= 0)
        {
            GetPaths(next + new Vector2(1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(-1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, 1), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, -1), newPath, remainingMoves);
        }
    }

    protected IEnumerator SelectedPath()
    {
        yield return StartCoroutine(FollowPath());
        GetAvailableTargets();
        GetAvailableActions();
        SelectAction();
    }

    protected override void UpdateObjectives()
    {
        currentObjective = new Objective(null, null);
    }

    protected override void FindPath()
    {
        ShowPaths();
        gettingMove = true;
        Debug.Log("Player waiting for move input");
    }

    private bool GetMoveInput()
    {
        Vector2 coords = GetMousePosition();

        if (paths.ContainsKey(coords))
        {
            paths.TryGetValue(coords, out pathToObjective);
            HighlightPath(pathToObjective);

            if (Input.GetMouseButtonDown(0))
            {
                HideIndicators();
                return false;
            }
        }
        else
        {
            UnhighlightPaths();
        }

        return true;
    }

    protected void SelectAction()
    {
        if (actions.Count > 1)
            SetupActionMenu();
        else
            GetActionInput(actions.ElementAt(0));
    }

    protected void GetActionInput(string action)
    {
        HideActionMenu();
        currentObjective.action = action;
        if (currentObjective.action != "Wait")
            SelectTarget();
        else
            Act();
    }

    protected void SelectTarget()
    {
        ShowObjects(GetObjects());
        gettingTarget = true;
        Debug.Log("Player waiting for target input");
    }

    private List<InteractableObject> GetObjects()
    {
        switch (currentObjective.action)
        {
            case "Attack":
                return attackableObjects;
            case "Heal":
                return healableObjects;
            case "Talk":
                return talkableObjects;
            case "Door":
                return openableDoors;
            case "Unlock":
                return unlockableDoors;
            default:
                throw new Exception("Unknown objective");
        }
    }

    private bool GetTargetInput()
    {
        Vector2 coords = GetMousePosition();
        
        List<InteractableObject> objects = GetObjects();
        foreach (InteractableObject o in objects)
        {
            if ((Vector2)o.transform.position == coords)
            {
                HighlightPath(new Vector2[] { coords });

                if (Input.GetMouseButtonDown(0))
                {
                    currentObjective.target = o;
                    HideIndicators();
                    return false;
                }
            }
            else
                UnhighlightPath(new Vector2[] { o.transform.position });
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

    protected override void SelectItem(String type)
    {
        ShowInventory(type, type == "Weapon" ? GetDistance(currentObjective.target) : 0);
        inventoryUI.SetActive(true);
        Debug.Log("Player waiting for item input");
    }

    public override void ChooseItem(HoldableObject item)
    {
        HideInventory();
        base.ChooseItem(item);
        StartCoroutine(EndTurn());
    }

    protected override void OnTriggerEnter2D(Collider2D other)
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

    private void CheckIfGameOver()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
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

    private void SetupActionMenu()
    {
        int index = 0,
            buttonHeight = 30,
            buttonWidth = 160,
            height = (buttonHeight + 10) * actions.Count;
        RectTransform panelRectTransform = GameObject.Find("ActionPanel").transform.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(buttonWidth + 10, height);
        foreach (string action in actions)
        {
            GameObject button;
            actionButtons.TryGetValue(action, out button);
            button.SetActive(true);
            button.transform.position = new Vector2(Screen.width / 2, Screen.height / 2 + height / 2 - 5 - (buttonHeight + 10) * index - buttonHeight / 2);
            index++;
        }

        actionMenu.alpha = 1f;
        actionMenu.blocksRaycasts = true;
        Debug.Log("Player waiting for act input");
    }

    private void HideActionMenu()
    {
        actionMenu.alpha = 0f;
        actionMenu.blocksRaycasts = false;
        foreach (GameObject button in actionButtons.Values)
        {
            button.SetActive(false);
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

    public void ShowObjects(List<InteractableObject> objects)
    {
        HideIndicators();

        foreach (InteractableObject o in objects)
        {
            indicators.Add(Instantiate(tileIndicator, o.transform.position, Quaternion.identity));
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

    private void UnhighlightPath(Vector2[] path)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            if (path.Contains((Vector2)indicator.transform.position))
            {
                indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
    }

    private void UnhighlightPaths()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    private void ShowInventory(String type, int range = 0)
    {
        List<HoldableObject> items;
        inventory.TryGetValue(type, out items);
        int j = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items != null && (range == 0 || items[i].range >= range))
            {
                slots[j].AddItem(items[i]);
                j++;
            }
        }
        while (j < slots.Length)
        {
            slots[j].ClearSlot();
            j++;
        }
    }

    private void HideInventory()
    {
        inventoryUI.SetActive(false);
    }
}
