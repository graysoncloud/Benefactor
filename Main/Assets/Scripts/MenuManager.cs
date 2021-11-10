using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;
using UnityEditor;

public class MenuManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public Transform itemsParent;
    public InventorySlot[] slots;
    public GameObject playerStats;
    public GameObject portrait;
    public Text characterName;
    public Text healthText;
    public Text movesText;
    public GameObject mouseIndicatorSprite;
    public GameObject tileIndicatorSprite;
    public List<GameObject> indicators;
    private GameObject mouseIndicator;
    public float unhighlightedAlpha;
    public float highlightedAlpha;
    public Color defaultColor;
    public Color defaultPlayerColor;
    public Color defaultEnemyColor;
    public CanvasGroup actionMenu;
    private Dictionary<String, GameObject> actionButtons;
    public GameObject backButton;

    void Awake()
    {
        // portrait = GameObject.Find("StatsPortrait");
        // characterName = GameObject.Find("StatsCharacterName"); //.GetComponent<Text>();
        // healthText = GameObject.Find("StatsHealthText").GetComponent<Text>();
        // healthText.text = "Health: " + health;
        
        // actionMenu = GameObject.Find("ActionPanel").GetComponent<CanvasGroup>();
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

        // inventoryUI = GameObject.Find("InventoryParent");
        inventoryUI.transform.position = new Vector2(Screen.width / 2, Screen.height / 4);
        // itemsParent = GameObject.Find("Inventory").GetComponent<Transform>();
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        HideInventory();

        // playerStats = GameObject.Find("PlayerStats");
        HidePlayerStats();

        // backButton = GameObject.Find("BackButton");
        backButton.transform.position = new Vector2(Screen.width*0.9f, Screen.height*0.1f);
        HideBackButton();

        defaultColor.a = unhighlightedAlpha;
        defaultPlayerColor.a = unhighlightedAlpha;
        defaultEnemyColor.a = unhighlightedAlpha;
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
            button.GetComponent<RectTransform>().transform.localPosition = new Vector2(0, (0 + height/2 - spacing - buttonHeight/2 - (buttonHeight + spacing) * index));
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

    public void ShowPlayerStats(InteractableObject target)
    {
        Character character = target.GetComponent<Character>();
        portrait.GetComponent<Image>().sprite = (character != null) ? character.portrait : target.GetComponent<SpriteRenderer>().sprite;
        characterName.GetComponent<Text>().text = (character != null) ? character.name : target.name.Replace("(Clone)", "");;
        healthText.GetComponent<Text>().text = "❤️ " + target.GetHealth().ToString() + "/" + target.maxHealth;
        movesText.GetComponent<Text>().text = (character != null) ? ("➤  " + character.totalMoves.ToString()) : "";
        playerStats.SetActive(true);
        // ShowBackButton();
    }

    public void HidePlayerStats()
    {
        playerStats.SetActive(false);
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
            ShowIndicator(entry.Key, defaultPlayerColor);
        }
    }

    public void ShowIndicator(Vector2 coords, Color color)
    {
        indicators.Add(Instantiate(tileIndicatorSprite, coords, Quaternion.identity));
        indicators[indicators.Count - 1].GetComponent<SpriteRenderer>().material.color = color;
    }

    // public void HideIndicator(Vector2 coords)
    // {
    //     foreach (GameObject indicator in indicators)
    //     {
    //         if ((Vector2)indicator.transform.position == coords) {
    //             indicators.Remove(indicator);
    //             Destroy(indicator);
    //         }
    //     }
    // }

    public void ShowMouseIndicator(Vector2 coords)
    {
        mouseIndicator = Instantiate(mouseIndicatorSprite, coords, Quaternion.identity);
        mouseIndicator.GetComponent<SpriteRenderer>().material.color = defaultColor;
        InteractableObject overlap = mouseIndicator.GetComponent<MouseIndicator>().FindInteractableObject();
        if (overlap != null)
        {
            ShowPlayerStats(overlap);
        }
        else
        {
            HidePlayerStats();
        }
    }

    public void HideMouseIndicator()
    {
        Destroy(mouseIndicator);
    }

    public void ShowObjects(List<InteractableObject> objects)
    {
        HideIndicators();
        Color color = defaultColor;

        foreach (InteractableObject o in objects)
        {
            if (o.gameObject.tag == "Character") {
                color = o.gameObject.GetComponent<Player>().playable ? defaultPlayerColor : defaultEnemyColor;
            }
            ShowIndicator(o.transform.position, defaultColor);
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
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            if (path.Contains((Vector2)indicator.transform.position))
            {
                currentColor.a = highlightedAlpha;
            }
            else
            {
                currentColor.a = unhighlightedAlpha;
            }
            indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
        }
    }

    public void UnhighlightPath(Vector2[] path)
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            if (path.Contains((Vector2)indicator.transform.position))
            {
                currentColor.a = unhighlightedAlpha;
                indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
            }
        }
    }

    public void UnhighlightPaths()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            Color currentColor = indicator.GetComponent<SpriteRenderer>().material.color;
            currentColor.a = unhighlightedAlpha;
            indicator.GetComponent<SpriteRenderer>().material.color = currentColor;
        }
    }

    // public void UpdateHealth(double health)
    // {
    //     healthText.text = "Health: " + health;
    // }

    public static void ActionButtonPressed(String action)
    {
        GameManager.instance.activeCharacter.GetActionInput(action);
    }

    public static void BackButtonPressed()
    {
        GameManager.instance.activeCharacter.Back();
    }
}
