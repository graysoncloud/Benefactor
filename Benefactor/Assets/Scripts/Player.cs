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
                Debug.Log("Horizontal: " + horizontal + ", Vertical: " + vertical);
            }
        }

        base.Update();
    }

    bool GetInput()
    {
        horizontal = 0;
        vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

        return (horizontal == 0 && vertical == 0);
    }

    protected override void GetTarget()
    {
        gettingTarget = true;
        Debug.Log("Player waiting for input");
    }

    protected override void TrackTarget()
    {
        RaycastHit2D hit;
        Move(horizontal, vertical, out hit);
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
