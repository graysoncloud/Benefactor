using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : Character
{
    public float restartLevelDelay = 1f;
    public Text rationaleText;
    public Text healthText;
    public List<Vector3> tilesVisited;

    public double rationale;

    // Start is called before the first frame update
    protected override void Start()
    {
        moveTime = 0.5f;
        base.Start();
        maxHealth = 10;
        moves = 3;
        movesUsed = 0;

        health = GameManager.instance.playerHealth;
        rationale = GameManager.instance.playerRationale;

        if (GameManager.instance.level == 1)
        {
            health = maxHealth;
            rationale = 50;
        }
        
        
        healthText.text = "Health: " + health;
        rationaleText.text = "Rationale: " + rationale;
    }

    private void OnDisable()
    {
        GameManager.instance.defaultReputation = reputation;
        GameManager.instance.playerHealth = health;
        GameManager.instance.playerRationale = rationale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;
        if (movesUsed >= moves) return;
        if (transform.position.x % 1 != 0 || transform.position.y % 1 != 0) return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            AttemptMove<InteractableObject>(horizontal, vertical);
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        isMoving = true;

        base.AttemptMove<T>(xDir, yDir);

        if (!tilesVisited.Contains(transform.position))
        {
            tilesVisited.Add(transform.position);
            movesUsed++;
        }

        CheckIfGameOver();

        isMoving = false;


        //GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            rationale += 3;
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            heal(3);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            heal(1);
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        InteractableObject hitObject = component as InteractableObject;
        hitObject.takeDamage(strength * (rationale / 50));

        animator.SetTrigger("playerChop");

        if (hitObject.health <= 0)
        {
            rationale -= (hitObject.reputation * 0.1);
            rationaleText.text = "Rationale: " + rationale;
        }
    }

    public void EndTurn()
    {
        GameManager.instance.playersTurn = false;
        tilesVisited.Clear();
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void takeDamage (double loss)
    {
        base.takeDamage(loss);
        healthText.text = "Health: " + health;
        animator.SetTrigger("playerHit");
        CheckIfGameOver();
    }

    public override void heal (int amount)
    {
        base.heal(amount);
        healthText.text = "Health: " + health;
    }

    private void CheckIfGameOver ()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
}
