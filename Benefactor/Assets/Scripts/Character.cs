using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Character : InteractableObject
{
    public float moveTime;
    public int moves;
    public int strength;

    protected Animator animator;
    private float inverseMoveTime;
    private Transform target;
    private bool skipMove;

    // Start is called before the first frame update
    protected override void Start()
    {
        strength = 1;

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;

        GameManager.instance.AddCharacterToList(this);
        target = GameObject.FindGameObjectWithTag("Player").transform; //update with unique objective

        base.Start();
        reputation = GameManager.instance.defaultReputation;
    }

    /**
     * Ideally, MoveTo will utilize a SingleMove() function to provide smooth movement one tile at a time, but this will function for the MVP
     * @param toMoveTo The intended destination for the object
     */
    public void MoveTo(Vector3 toMoveTo)
    {
        transform.position = toMoveTo;
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {

        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, Collisions);
        boxCollider.enabled = true;

        if (skipMove)
        {
            skipMove = false;
            return false;
        }
        skipMove = true;

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

    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }

    public void MoveCharacter()
    {
        int xDir = 0;
        int yDir = 0;

        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            yDir = target.position.y < transform.position.y ? -1 : 1;
        else
            xDir = target.position.x < transform.position.x ? -1 : 1;

        AttemptMove<Player>(xDir, yDir);
    }

    protected virtual void OnCantMove<T>(T component)
    {
        InteractableObject hitObject = component as InteractableObject;
        hitObject.takeDamage(strength);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
