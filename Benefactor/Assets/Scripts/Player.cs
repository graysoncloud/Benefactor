using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : Character
{
    public Text rationaleText;
    public Text healthText;

    private int horizontal;
    private int vertical;

    // Start is called before the first frame update
    protected override void Start()
    {
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

    //bool GetInput()
    //{
    //    horizontal = 0;
    //    vertical = 0;

    //    horizontal = (int)Input.GetAxisRaw("Horizontal");
    //    vertical = (int)Input.GetAxisRaw("Vertical");

    //    if (horizontal != 0)
    //        vertical = 0;

    //    return (horizontal == 0 && vertical == 0);
    //}

    bool GetInput()
    {
        int x;
        int y;
        Vector2 coords = new Vector2();
        if (Input.GetMouseButtonDown(0))
        {
            x = (int)((Input.mousePosition.x - Screen.width/2 - 28)/56 + transform.position.x + 1); //Don't know actual tile size yet!
            y = (int)((Input.mousePosition.y - Screen.height/2 - 28)/56 + transform.position.y + 1);
            coords = new Vector2(x, y);
            Debug.Log("Mouse down at: " + coords);

            if (paths.ContainsKey(coords))
            {
                target = coords;
                return false;
            }
        }

        return true;
    }

    protected override void GetTarget()
    {
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
}
