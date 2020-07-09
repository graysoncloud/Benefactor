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
    public int defaultReputation = 50;
    [HideInInspector] public bool playersTurn = true;

    public double playerHealth;
    public double playerRationale;

    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    private List<Character> characters;
    private bool charactersMoving;
    private bool doingSetup;

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
        //InitGame();

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
    }

    //    private void OnLevelWasLoaded(int index)
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
        if (playersTurn || charactersMoving || doingSetup)
            return;

        StartCoroutine(MoveCharacters());
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

    IEnumerator MoveCharacters()
    {
        charactersMoving = true;
        yield return new WaitForSeconds(turnDelay);
        //if (characters.Count == 0)
        //{
        //    yield return new WaitForSeconds(turnDelay);
        //}

        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].health > 0 && characters[i].GetType() != typeof(Player))
                characters[i].MoveCharacter();

            //yield return new WaitForSeconds(characters[i].moveTime);
        }

        // Set up players turn
        GameObject.FindObjectOfType<Player>().movesUsed = 0;
        GameObject.FindObjectOfType<Player>().tilesVisited.Add(GameObject.FindObjectOfType<Player>().transform.position);
        playersTurn = true;
        charactersMoving = false;
    }

}
