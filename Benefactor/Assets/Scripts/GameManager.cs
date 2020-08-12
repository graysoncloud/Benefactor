using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;

public class GameManager : MonoBehaviour
{
    public List<List<Node>> Grid;

    public int level = 0;
    public float levelStartDelay = 2f;
    public float turnDelay;
    public static GameManager instance = null;

    public double defaultRationale;
    public int defaultReputation;
    public int defaultMoves;
    public int defaultStrength;
    public float defaultMoveTime;
    public float defaultActionDelay;

    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    public List<Character> characters;
    private bool doingSetup;

    public int activeCharacterIndex;
    public Character activeCharacter;
    public int round;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        characters = new List<Character>();
        boardScript = GetComponent<BoardManager>();

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
        if (activeCharacterIndex >= characters.Count)
        {
            nextRound();
            yield break;
        }
        activeCharacter = characters[activeCharacterIndex];
        Debug.Log("Active Character Index: " + activeCharacterIndex);
        yield return new WaitForSeconds(turnDelay);
        StartCoroutine(activeCharacter.StartTurn());
    }

    public void nextRound()
    {
        round++;
        activeCharacterIndex = -1;
        StartCoroutine(nextTurn());
    }

    public void AddCharacterToList(Character script)
    {
        characters.Add(script);
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
}
