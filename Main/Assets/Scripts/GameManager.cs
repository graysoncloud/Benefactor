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

    public int level = 0;
    public float levelStartDelay;
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
        if (!doingSetup && gettingNextCharacter)
        {
            gettingNextCharacter = GameObject.Find("MouseManager").GetComponent<MouseManager>().GetNextCharacter(GetPlayableCharacters());
            if (!gettingNextCharacter)
            {
                StartCoroutine(StartTurn());
            }
        }

        if (!doingSetup && activeCharacter == null)
        {
            StartCoroutine(NextTurn());
        }
    }

    public void FinishSetup() {
        doingSetup = false;
    }

    public void CheckRoofs() {
        foreach (BoardManager.Roof roof in boardScript.GetRoofs()) {
            bool overPlayableCharacter = false;
            foreach (Player player in GetPlayableCharacters(true)) {
                if (roof.positions.Contains(new Vector2Int((int) player.gameObject.transform.position.x, (int) player.gameObject.transform.position.y))) {
                    overPlayableCharacter = true;
                }
            }
            roof.tileRenderer.enabled = !overPlayableCharacter;
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
        // levelText.text = "Day " + level;
        levelText.text = "Demo Sandbox";
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        characters.Clear();
        Grid = boardScript.SetupScene(level);
        round = 0;
        SoundManager.instance.PlayMusic(1);
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
        GameObject.Find("Main Camera").GetComponent<FollowPlayer>().FollowMouse();
        CameraTarget(GetPlayableCharacters()[0].gameObject); //temp until camera follows mouse
    }

    public void AISelectCharacter()
    {
        List<Player> nonplayableCharacters = GetNonplayableCharacters();

        int i = new System.Random().Next(nonplayableCharacters.Count);
        activeCharacter = nonplayableCharacters[i];

        StartCoroutine(StartTurn(true));
    }

    public IEnumerator NextTurn()
    {
        if (GetPlayableCharacters().Count == 0)
        {
            Debug.Log("NO PLAYABLE CHARACTERS");
            if (playerStart)
                playerTurn = false;
            else
            {
                NextRound();
                yield break;
            }
        }

        if (GetNonplayableCharacters().Count == 0)
        {
            Debug.Log("NO ENEMIES");
            if (!playerStart)
                playerTurn = true;
            else
            {
                NextRound();
                yield break;
            }
        }

        if (playerTurn)
        {
            PlayerSelectCharacter();
        } else {
            //yield return new WaitForSeconds(turnDelay);
            AISelectCharacter();
        }
    }

    private IEnumerator StartTurn(bool delay = false)
    {
        Debug.Log("Active Character: " + activeCharacter);
        CameraTarget(activeCharacter.gameObject);
        GameObject.Find("Main Camera").GetComponent<FollowPlayer>().FollowTarget();
        if (delay)
            yield return new WaitForSeconds(turnDelay);
        activeCharacter.StartTurn();
    }

    public void NextRound()
    {
        round++;
        playerTurn = playerStart;

        foreach (Player character in characters)
        {
            character.hasGone = false;
        }

        StartCoroutine(NextTurn());
    }

    public void AddCharacterToList(Player character)
    {
        characters.Add(character);
    }

    public bool IsPlayableCharacter(Player character) {
        return GetPlayableCharacters().Contains(character);
    }

    private List<Player> GetPlayableCharacters(bool ignoreGone = false)
    {
        List<Player> playableCharacters = new List<Player>();
        foreach (Player character in characters)
        {
            if (character.playable && (ignoreGone || !character.hasGone))
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
