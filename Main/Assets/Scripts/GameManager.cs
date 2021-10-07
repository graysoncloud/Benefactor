using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    public List<List<Node>> Grid;
    public List<List<Roof>> Roofs;

    public int level = 0;
    public float levelStartDelay = 2f;
    public float turnDelay;
    public static GameManager instance = null;

    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    public List<Player> characters;
    private bool doingSetup;

    public int activeCharacterIndex;
    public Player activeCharacter;
    public bool dialogueInProgress;
    public int round;

    public GameObject inventoryUI;
    public Transform itemsParent;
    public InventorySlot[] slots;
    public Text rationaleText;
    public Text healthText;
    public GameObject tileIndicator;
    public List<GameObject> indicators;
    private CanvasGroup actionMenu;
    private Dictionary<String, GameObject> actionButtons;
    private GameObject backButton;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        characters = new List<Player>();
        boardScript = GetComponent<BoardManager>();
        dialogueInProgress = false;

        rationaleText = GameObject.Find("RationaleText").GetComponent<Text>();
        // rationaleText.text = "Rationale: " + rationale;
        healthText = GameObject.Find("HealthText").GetComponent<Text>();
        // healthText.text = "Health: " + health;
        
        actionMenu = GameObject.Find("ActionPanel").GetComponent<CanvasGroup>();
        actionButtons = new Dictionary<String, GameObject>();
        actionButtons.Add("Attack", GameObject.Find("AttackButton"));
        actionButtons.Add("Talk", GameObject.Find("TalkButton"));
        actionButtons.Add("Heal", GameObject.Find("HealButton"));
        actionButtons.Add("Door", GameObject.Find("DoorButton"));
        actionButtons.Add("Unlock", GameObject.Find("UnlockButton"));
        actionButtons.Add("Lever", GameObject.Find("LeverButton"));
        actionButtons.Add("Loot", GameObject.Find("LootButton"));
        actionButtons.Add("Steal", GameObject.Find("StealButton"));
        actionButtons.Add("Wait", GameObject.Find("WaitButton"));
        HideActionMenu();

        inventoryUI = GameObject.Find("InventoryParent");
        inventoryUI.transform.position = new Vector2(Screen.width / 2, Screen.height / 4);
        itemsParent = GameObject.Find("Inventory").GetComponent<Transform>();
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        HideInventory();

        backButton = GameObject.Find("BackButton");
        backButton.transform.position = new Vector2(Screen.width*0.9f, Screen.height*0.1f);
        HideBackButton();
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        level++;
        InitGame();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void InitGame()
    {
        doingSetup = true;

        levelImage = GameObject.Find("LevelImage"); 
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        characters.Clear();
        Grid = boardScript.SetupScene(level);
        Roofs = boardScript.GetRoofs();
        round = 0;
        activeCharacterIndex = -1;
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver()
    {
        levelText.text = "After " +  level + " days, you perished.";
        levelImage.SetActive(true);
        enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (characters.Count > 0) { doingSetup = false;  } //kinda sketchy way of making sure characters have been loaded in, might change to a time delay?
        if (!doingSetup && activeCharacterIndex == -1)
        {
            StartCoroutine(nextTurn());
        }
    }

    public IEnumerator nextTurn()
    {
        activeCharacterIndex++;
        Debug.Log("Characters: " + characters);
        if (activeCharacterIndex >= characters.Count)
        {
            nextRound();
            yield break;
        }
        activeCharacter = characters[activeCharacterIndex];
        if (!activeCharacter.playable && activeCharacter.currentObjective == null && activeCharacter.objectives.Count == 0)
        {
            Debug.Log("Active Character: " + activeCharacter);
            StartCoroutine(nextTurn());
            yield break;
        }

        Debug.Log("Active Character Index: " + activeCharacterIndex);

        CameraTarget(activeCharacter.gameObject);

        yield return new WaitForSeconds(turnDelay);
        activeCharacter.StartTurn();
    }

    public void nextRound()
    {
        round++;
        activeCharacterIndex = -1;
        StartCoroutine(nextTurn());
    }

    public void AddCharacterToList(Player character)
    {
        characters.Add(character);
    }

    public void RemoveDeadCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].GetHealth() <= 0)
                characters.RemoveAt(i);
        }
    }

    public void UpdateNode(Vector2 position, bool damageable, float health)
    {
        Grid[(int)position.x][(int)position.y] = new Node(position, damageable, health + 1);
    }

    public void CameraTarget(GameObject toTarget)
    {
        Camera.main.GetComponent<FollowPlayer>().Target(toTarget);
    }

    public void SetupActionMenu(SortedSet<String> actions)
    {
        Vector2 position = new Vector2(Screen.width / 2, Screen.height / 3);
        GameObject buttonForScale;
        actionButtons.TryGetValue("Attack", out buttonForScale);
        float buttonHeight = buttonForScale.GetComponent<RectTransform>().rect.height,
            buttonWidth = buttonForScale.GetComponent<RectTransform>().rect.width,
            spacing = buttonHeight*0.3f,
            width  = buttonWidth + spacing * 2,
            height = (buttonHeight + spacing) * actions.Count + spacing;
        RectTransform panelRectTransform = GameObject.Find("ActionPanel").transform.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(width, height);
        panelRectTransform.transform.position = position;
        int index = 0;
        foreach (string action in actions)
        {
            GameObject button;
            actionButtons.TryGetValue(action, out button);
            button.SetActive(true);
            button.transform.position = new Vector2(position.x, position.y + height/2 - spacing - buttonHeight/2 - (buttonHeight + spacing) * index);
            Debug.Log(button.transform.position);
            index++;
        }

        actionMenu.alpha = 1f;
        actionMenu.blocksRaycasts = true;
        Debug.Log("Player waiting for act input");
    }

    public void HideActionMenu()
    {
        actionMenu.alpha = 0f;
        actionMenu.blocksRaycasts = false;
        foreach (GameObject button in actionButtons.Values)
        {
            button.SetActive(false);
        }
    }

    public List<HoldableObject> SortedInventory(String type, List<HoldableObject> inventory)
    {
        return inventory.FindAll(e => e.type == type);
    }

    public void ShowInventory(String type, List<HoldableObject> inventory, int range = 0, List<HoldableObject> items = null)
    {
        if (items == null)
            items = SortedInventory(type, inventory);
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

        inventoryUI.SetActive(true);
        ShowBackButton();
    }

    public void HideInventory()
    {
        inventoryUI.SetActive(false);
    }

    public void ShowBackButton()
    {
        backButton.SetActive(true);
    }

    public void HideBackButton()
    {
        backButton.SetActive(false);
    }

    public void ShowPaths(Dictionary<Vector2, Vector2[]> paths)
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

    public void HideIndicators()
    {
        foreach (GameObject indicator in indicators)
        {
            Destroy(indicator);
        }
        indicators.Clear();
    }

    public void HighlightPath(Vector2[] path)
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

    public void UnhighlightPath(Vector2[] path)
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

    public void UnhighlightPaths()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    public void UpdateHealth(double health)
    {
        healthText.text = "Health: " + health;
    }
}
