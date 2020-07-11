using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int level = 0;
    public float levelStartDelay = 2f;
    public float turnDelay = .1f;
    public static GameManager instance = null;
    [HideInInspector] public bool playersTurn = true;

    public double defaultHealth;
    public double defaultRationale;
    public int defaultReputation;
    public int defaultMoves;
    public int defaultStrength;
    public float defaultMoveTime;
    public float defaultActionDelay;

    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    private List<Character> characters;
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
        boardScript.SetupScene(level);
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
        if (characters.Count > 0) { doingSetup = false;  }
        if (!doingSetup && activeCharacterIndex == -1)
        {
            nextTurn();
        }
    }

    public void nextTurn()
    {
        activeCharacterIndex++;
        Debug.Log("Index: " + activeCharacterIndex);

        if (activeCharacterIndex >= characters.Count)
        {
            nextRound();
            return;
        }

        activeCharacter = characters[activeCharacterIndex];

        activeCharacter.StartTurn();
    }

    public void nextRound()
    {
        round++;
        activeCharacterIndex = -1;
        nextTurn();
    }

    public void AddCharacterToList(Character script)
    {
        characters.Add(script);
    }

    public void RemoveDeadCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].health <= 0)
                characters.RemoveAt(i);
        }
    }

    //IEnumerator MoveCharacters()
    //{
    //    charactersMoving = true;
    //    yield return new WaitForSeconds(turnDelay);
    //    //if (characters.Count == 0)
    //    //{
    //    //    yield return new WaitForSeconds(turnDelay);
    //    //}

    //    for (int i = 0; i < characters.Count; i++)
    //    {
    //        if (characters[i].health > 0 && characters[i].GetType() != typeof(Player))
    //            characters[i].isTurn = true;
    //            characters[i].Act();
    //            while (characters[i].isTurn) { }

    //        //yield return new WaitForSeconds(characters[i].moveTime);
    //    }

    //    // Set up players turn
    //    GameObject.FindObjectOfType<Player>().movesUsed = 0;
    //    GameObject.FindObjectOfType<Player>().tilesVisited.Add(GameObject.FindObjectOfType<Player>().transform.position);
    //    playersTurn = true;
    //    charactersMoving = false;
    //}

}
