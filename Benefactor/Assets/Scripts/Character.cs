﻿using System;
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
    public bool isMoving;
    public int movesUsed;

    protected Animator animator;
    private float inverseMoveTime;
    protected Vector2 target;
    protected Transform objective;
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
        isMoving = false;

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        paths = new Dictionary<Vector2, Vector2[]>();

        GameManager.instance.AddCharacterToList(this);

        base.Start();
    }

    virtual protected void Update()
    {

    }

    public void StartTurn()
    {
        isTurn = true;
        movesUsed = 0;
        gettingTarget = true;
        StartCoroutine(GetPaths());
    }

    protected IEnumerator GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], moves);
        yield return new WaitForSeconds(moveTime);
        GetTarget();
    }

    protected void GetPaths(Vector2 next, Vector2[] path, int remainingMoves)
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
        else
        {
            paths.TryGetValue(next, out path);
            if (newPath.Length < path.Length)
            {
                paths.Remove(next);
                paths.Add(next, newPath);
            }
        }

        remainingMoves--;
        if (remainingMoves >= 0)
        {
            GetPaths(next + new Vector2(1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(-1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, 1), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, -1), newPath, remainingMoves);
        }
    }

    protected void LogPaths()
    {
        String actions = "Available Moves (" + paths.Count + "): ";
        foreach(KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            actions += entry.Key + ", ";
        }
        Debug.Log(actions);
    }

    virtual protected void GetTarget()
    {
        objective = GameObject.FindGameObjectWithTag("Player").transform; //update with unique objective
        if (paths.ContainsKey(objective.position))
        {
            target = objective.position;
        } else
        {
            target = transform.position;
        }
        gettingTarget = false;
        StartCoroutine(FollowPath());
    }


    //public IEnumerator Act()
    //{
    //    isActing = true;
    //    movesUsed++;
    //    Debug.Log("Moves Used: " + movesUsed + ", Total Moves: " + moves);
    //    TrackTarget();
    //    yield return new WaitForSeconds(moveTime);
    //    isActing = false;
    //    if (movesUsed < moves)
    //    {
    //        GetTarget();
    //    }
    //}

    protected IEnumerator FollowPath()
    {
        if (target != (Vector2)transform.position)
        {
            isMoving = true;
            Vector2 end = target;
            Vector2[] path;
            paths.TryGetValue(end, out path);
            foreach (Vector2 coords in path)
            {
                if (coords != (Vector2)transform.position)
                {
                    movesUsed++;
                    StartCoroutine(SmoothMovement(coords));
                    yield return new WaitForSeconds(moveTime);
                }
            }
            isMoving = false;
        }
        Act();
    }

    protected void Act()
    {
        //isActing = true;
        endTurn();
    }

    protected void endTurn()
    {
        isTurn = false;
        GameManager.instance.nextTurn();
    }

    //virtual protected void TrackTarget()
    //{
    //    int xDir = 0;
    //    int yDir = 0;

    //    if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
    //        yDir = target.position.y < transform.position.y ? -1 : 1;
    //    else
    //        xDir = target.position.x < transform.position.x ? -1 : 1;

    //    RaycastHit2D hit;
    //    Move(xDir, yDir, out hit);
    //}

    //protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    //{

    //    Vector2 start = transform.position;
    //    Vector2 end = start + new Vector2(xDir, yDir);

    //    boxCollider.enabled = false;
    //    hit = Physics2D.Linecast(start, end, Collisions);
    //    boxCollider.enabled = true;

    //    if (hit.transform == null)
    //    {
    //        StartCoroutine(SmoothMovement(end));
    //        return true;
    //    }

    //    return false;
    //}

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