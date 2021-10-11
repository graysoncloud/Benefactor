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

    public bool gettingNextCharacter;
    public Player activeCharacter;
    public bool dialogueInProgress;
    public int round;
    private bool playerTurn;
    public bool playerStart;

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
        playerTurn = playerStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (characters.Count > 0) { doingSetup = false;  } //kinda sketchy way of making sure characters have been loaded in, might change to a time delay?

        if (!doingSetup && gettingNextCharacter)
        {
            gettingNextCharacter = GameObject.Find("MouseManager").GetComponent<MouseManager>().GetNextCharacter(GetPlayableCharacters());
            if (!gettingNextCharacter)
            {
                startTurn();
            }
        }

        if (!doingSetup && activeCharacter == null)
        {
            StartCoroutine(nextTurn());
        }
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

    public void PlayerSelectCharacter()
    {
        Dictionary<Vector2, Vector2[]> paths = new Dictionary<Vector2, Vector2[]>();
        foreach (Player character in GetPlayableCharacters())
        {
            paths.Add((Vector2)character.transform.position, new Vector2[] { (Vector2)character.transform.position });
        }
        GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowPaths(paths);

        gettingNextCharacter = true;
        CameraTarget(GetPlayableCharacters()[0].gameObject); //temp until camera follows mouse
    }

    public void AISelectCharacter()
    {
        List<Player> nonplayableCharacters = GetNonplayableCharacters();
        Debug.Log("# of nonplayables: " + nonplayableCharacters.Count);

        int i = new System.Random().Next(nonplayableCharacters.Count);
        activeCharacter = nonplayableCharacters[i];

        startTurn();
    }

    public IEnumerator nextTurn()
    {
        if (GetPlayableCharacters().Count == 0)
        {
            if (playerStart)
                playerTurn = false;
            else
            {
                nextRound();
                yield break;
            }
        }

        if (GetNonplayableCharacters().Count == 0)
        {
            if (!playerStart)
                playerTurn = true;
            else
            {
                nextRound();
                yield break;
            }
        }

        if (playerTurn)
        {
            PlayerSelectCharacter();
        } else {
            yield return new WaitForSeconds(turnDelay);
            AISelectCharacter();
        }
    }

    private void startTurn()
    {
        Debug.Log("Active Character: " + activeCharacter);
        CameraTarget(activeCharacter.gameObject);
        activeCharacter.StartTurn();
    }

    public void nextRound()
    {
        round++;
        playerTurn = playerStart;

        foreach (Player character in characters)
        {
            character.hasGone = false;
        }

        StartCoroutine(nextTurn());
    }

    public void AddCharacterToList(Player character)
    {
        characters.Add(character);
    }

    private List<Player> GetPlayableCharacters()
    {
        List<Player> playableCharacters = new List<Player>();
        foreach (Player character in characters)
        {
            if (character.playable && !character.hasGone)
            {
                playableCharacters.Add(character);
            }
        }

        return playableCharacters;
    }

    private List<Player> GetNonplayableCharacters()
    {
        List<Player> nonplayableCharacters = new List<Player>();
        foreach (Player character in characters)
        {
            if (!character.playable && !character.hasGone)
            //if (!character.playable && !character.hasGone && character.currentObjective != null && character.objectives.Count > 0)
            {
                nonplayableCharacters.Add(character);
            }
        }

        return nonplayableCharacters;
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
