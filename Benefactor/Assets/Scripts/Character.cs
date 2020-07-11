using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Character : InteractableObject
{
    public float moveTime;
    public int moves;
    public float actionDelay;
    public int strength;
    public double rationale;
    //public int reputation;

    public bool isTurn;
    public bool gettingTarget;
    public bool isActing;
    public int movesUsed;

    protected Animator animator;
    private float inverseMoveTime;
    private Transform target;

    // Start is called before the first frame update
    protected override void Start()
    {
        health = GameManager.instance.defaultHealth;
        rationale = GameManager.instance.defaultRationale;
        //reputation = GameManager.instance.defaultReputation;
        moves = GameManager.instance.defaultMoves;
        strength = GameManager.instance.defaultStrength;
        moveTime = GameManager.instance.defaultMoveTime;
        actionDelay = GameManager.instance.defaultActionDelay;

        isTurn = false;
        gettingTarget = false;
        isActing = false;

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;

        GameManager.instance.AddCharacterToList(this);

        base.Start();
    }

    virtual protected void Update()
    {
        if (!isTurn || gettingTarget || isActing) return;
        //if (transform.position.x % 1 != 0 || transform.position.y % 1 != 0) return;
        if (movesUsed >= moves)
        {
            isTurn = false;
            GameManager.instance.nextTurn();
            return;
        }

        StartCoroutine(Act());
    }

    public void StartTurn()
    {
        movesUsed = 0;
        isTurn = true;
        GetTarget();
    }

    public void GetAvailableActions()
    {

    }

    virtual protected void GetTarget()
    {
        gettingTarget = true;
        target = GameObject.FindGameObjectWithTag("Player").transform; //update with unique objective
        gettingTarget = false;
    }

    public IEnumerator Act()
    {
        isActing = true;
        movesUsed++;
        Debug.Log("Moves Used: " + movesUsed + ", Total Moves: " + moves);
        TrackTarget();
        yield return new WaitForSeconds(moveTime);
        isActing = false;
        if (movesUsed < moves)
        {
            GetTarget();
        }
    }

    virtual protected void TrackTarget()
    {
        int xDir = 0;
        int yDir = 0;

        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            yDir = target.position.y < transform.position.y ? -1 : 1;
        else
            xDir = target.position.x < transform.position.x ? -1 : 1;

        RaycastHit2D hit;
        Move(xDir, yDir, out hit);
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {

        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, Collisions);
        boxCollider.enabled = true;

        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime * 10);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food")
        {
            Heal(3);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            Heal(1);
            other.gameObject.SetActive(false);
        }
    }

    protected void Attack<T>(T component)
    {
        InteractableObject hitObject = component as InteractableObject;
        hitObject.TakeDamage(strength * (rationale / 50));

        animator.SetTrigger("playerChop");

        if (hitObject.health <= 0)
        {
            rationale -= (hitObject.reputation * 0.1);
            //rationaleText.text = "Rationale: " + rationale;
        }
    }

    protected IEnumerator postActionDelay()
    {
        yield return new WaitForSeconds(actionDelay);
    }

}
