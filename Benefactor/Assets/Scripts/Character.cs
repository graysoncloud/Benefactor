using System;
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
    protected Dictionary<Vector2, Vector2[]> paths;

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
        paths = new Dictionary<Vector2, Vector2[]>();

        GameManager.instance.AddCharacterToList(this);

        base.Start();
    }

    virtual protected void Update()
    {
        if (!isTurn || gettingTarget || isActing) return;
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

    virtual protected void GetTarget()
    {
        gettingTarget = true;
        target = GameObject.FindGameObjectWithTag("Player").transform; //update with unique objective
        gettingTarget = false;
    }

    protected void GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], moves);
    }

    public void GetPaths(Vector2 next, Vector2[] path, int remainingMoves)
    {
        if (Array.Exists(path, element => element == next)) { return; }
        Vector2 previous = ((path.Length == 0) ? (Vector2) transform.position : path[path.Length - 1]);
        boxCollider.enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(previous, next, Collisions);
        boxCollider.enabled = true;
        if (hit.transform != null) { return; }

        Vector2[] newPath = new Vector2[path.Length + 1];
        Array.Copy(path, newPath, path.Length);
        newPath[newPath.Length - 1] = next;
        if (!paths.ContainsKey(next)) { paths.Add(next, newPath);  }

        remainingMoves--;
        if (remainingMoves >= 0)
        {
            GetPaths(next + new Vector2(1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(-1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, 1), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, -1), newPath, remainingMoves);
        }
    }

    public void LogPaths()
    {
        String actions = "Available Moves (" + paths.Count + "): ";
        foreach(KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            actions += entry.Key + ", ";
        }
        Debug.Log(actions);
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
