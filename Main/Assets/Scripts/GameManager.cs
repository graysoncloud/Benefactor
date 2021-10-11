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
}
