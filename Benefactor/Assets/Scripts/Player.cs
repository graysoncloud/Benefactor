using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class Player : Character
{
    public Text rationaleText;
    public Text healthText;

    public GameObject tileIndicator;
    public List<GameObject> indicators;

    //private BoardManager boardScript;

    // Start is called before the first frame update
    protected override void Start()
    {
        //boardScript = GetComponent<BoardManager>();
        base.Start();
        //healthText.text = "Health: " + health;
        //rationaleText.text = "Rationale: " + rationale;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (gettingTarget)
        {
            gettingTarget = GetInput();
            if (!gettingTarget)
            {
                StartCoroutine(FollowPath());
            }
        }
    }

    bool GetInput()
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
                target = coords;
                HidePaths();
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
        ShowPaths();
        Debug.Log("Player waiting for input");
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
        //healthText.text = "Health: " + health;
        animator.SetTrigger("playerHit");
        CheckIfGameOver();
    }

    public override void Heal (int amount)
    {
        base.Heal(amount);
        //healthText.text = "Health: " + health;
    }

    private void CheckIfGameOver ()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }

    public void ShowPaths()
    {
        HidePaths();

        foreach (KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            indicators.Add(Instantiate(tileIndicator, entry.Key, Quaternion.identity));
            indicators[indicators.Count - 1].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }

    public void HidePaths()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            indicator.SetActive(false);
        }
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

    public void UnhighlightPath()
    {
        foreach (GameObject indicator in indicators)
        {
            if (indicator == null) { break; }
            indicator.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }
}
