using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : Character
{
    public float restartLevelDelay = 1f;
    public Text reputationText;

    // Start is called before the first frame update
    protected override void Start()
    {

        //reputationText.text = "Reputation: " + reputation;

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.defaultReputation = reputation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall>(horizontal, vertical);
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {

        base.AttemptMove<T>(xDir, yDir);

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        } 
        //else if (other.tag == "Food")
        //{
        //    food += pointsPerFood;
        //    foodText.text = "Food: " + food;
        //    other.gameObject.SetActive(false);
        //}
    }

    protected override void OnCantMove<T>(T component)
    {
        base.OnCantMove<T>(component);
        animator.SetTrigger("playerChop");
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void takeDamage (int loss)
    {
        base.takeDamage(loss);
        animator.SetTrigger("playerHit");
        CheckIfGameOver();
    }

    private void CheckIfGameOver ()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
}
