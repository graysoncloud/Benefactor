using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Character : InteractableObject
{
    public class Objective
    {
        public Vector2 coords;
        public string action;

        public Objective(Vector2 coords, string action)
        {
            this.coords = coords;
            this.action = action;
        }
    }

    public float moveTime;
    public int moves;
    public float actionDelay;
    public int strength;
    public double rationale;
    public bool isTurn;
    public bool gettingMove;
    public bool gettingTarget;
    public bool isMoving;
    public int movesUsed;
    //public List<string> selfActions;

    protected Animator animator;
    private float inverseMoveTime;
    protected Queue<Objective> objectives;
    protected Objective currentObjective;
    protected Vector2 toMove;
    protected InteractableObject target; //target is the object the character is targeting this turn
    //protected Vector2[] pathToObjective;
    protected Dictionary<Vector2, Vector2[]> paths;
    protected List<InteractableObject> nearbyObjects;
    protected Dictionary<String, List<HoldableObject>> inventory;

    // Start is called before the first frame update
    protected override void Start()
    {
        rationale = GameManager.instance.defaultRationale;
        moves = GameManager.instance.defaultMoves;
        strength = GameManager.instance.defaultStrength;
        moveTime = GameManager.instance.defaultMoveTime;
        actionDelay = GameManager.instance.defaultActionDelay;

        isTurn = false;
        gettingMove = false;
        gettingTarget = false;
        isMoving = false;

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        objectives = new Queue<Objective>();
        paths = new Dictionary<Vector2, Vector2[]>();
        nearbyObjects = new List<InteractableObject>();
        inventory = new Dictionary<String, List<HoldableObject>>();

        GameManager.instance.AddCharacterToList(this);

        base.Start();
        receiveActions.Add("Talk");
        receiveActions.Add("Heal");
    }

    virtual protected void Update()
    {

    }

    public void StartTurn()
    {
        isTurn = true;
        movesUsed = 0;
        gettingMove = true;
        StartCoroutine(GetPaths());
    }

    protected IEnumerator GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], moves);
        yield return new WaitForSeconds(moveTime);
        GetMove();
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
            else
            {
                return;
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

    virtual protected void GetMove()
    {
        objectives.Enqueue(new Objective(GameObject.FindGameObjectWithTag("Player").transform.position, "Attack")); //update with unique objective
        currentObjective = objectives.Dequeue();
        //might update to do A* search eventually to find shortest path
        int minDistance = 999;
        foreach (KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            int distance = (int)(Math.Abs(entry.Key.x - currentObjective.coords.x) + Math.Abs(entry.Key.y - currentObjective.coords.y));
            if (distance < minDistance)
            {
                toMove = entry.Key;
                minDistance = distance;
            }
        }
        gettingMove = false;
        StartCoroutine(FollowPath());
    }

    protected IEnumerator FollowPath()
    {
        if (toMove != (Vector2)transform.position)
        {
            isMoving = true;
            Vector2 end = toMove;
            Vector2[] path;
            paths.TryGetValue(end, out path);
            foreach (Vector2 coords in path)
            {
                movesUsed++;
                StartCoroutine(SmoothMovement(coords));
                yield return new WaitForSeconds(moveTime);
            }
            isMoving = false;
        } else
        {
            yield return new WaitForSeconds(moveTime);
        }
        GetTarget();
    }

    virtual protected void GetTarget()
    {
        GetNearbyObjects();
        gettingTarget = true;
        if (nearbyObjects.Count > 1)
        {
            target = nearbyObjects[1];
            if (target.receiveActions.Count == 1)
                currentObjective.action = target.receiveActions[0];
            else //add AI choosing
                currentObjective.action = target.receiveActions[0];
        } else
        {
            currentObjective.action = "";
        }
        gettingTarget = false;
        Act();
    }

    virtual protected void Act()
    {
        switch(currentObjective.action)
        {
            case "Attack":
                Attack(target);
                break;
            case "Talk":
                TalkTo(target);
                break;
            case "Heal":
                target.Heal(1);
                break;
            default:
                break;
        }
        EndTurn();
    }

    protected void GetNearbyObjects()
    {
        nearbyObjects.Clear();
        nearbyObjects.Add(this);
        boxCollider.enabled = false;

        Vector2 next = (Vector2)transform.position + new Vector2(1, 0);
        RaycastHit2D hit = Physics2D.Linecast(transform.position, next, Collisions);
        InteractableObject hitObject;
        if (hit.transform != null)
        {
            hitObject = hit.transform.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.receiveActions.Count > 0)
            {
                nearbyObjects.Add(hitObject);
            }
        }

        next = (Vector2)transform.position + new Vector2(-1, 0);
        hit = Physics2D.Linecast(transform.position, next, Collisions);
        if (hit.collider != null)
        {
            hitObject = hit.transform.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.receiveActions.Count > 0)
            {
                nearbyObjects.Add(hitObject);
            }
        }

        next = (Vector2)transform.position + new Vector2(0, 1);
        hit = Physics2D.Linecast(transform.position, next, Collisions);
        if (hit.collider != null)
        {
            hitObject = hit.transform.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.receiveActions.Count > 0)
            {
                nearbyObjects.Add(hitObject);
            }
        }

        next = (Vector2)transform.position + new Vector2(0, -1);
        hit = Physics2D.Linecast(transform.position, next, Collisions);
        if (hit.collider != null)
        {
            hitObject = hit.transform.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.receiveActions.Count > 0)
            {
                nearbyObjects.Add(hitObject);
            }
        }

        boxCollider.enabled = true;
    }

    protected void EndTurn()
    {
        isTurn = false;
        StartCoroutine(GameManager.instance.nextTurn());
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
        if (other.gameObject.GetComponent<HoldableObject>() != null)
        {
            Pickup(other.gameObject.GetComponent<HoldableObject>());
        }

        //if (other.tag == "Food")
        //{
        //    //Heal(3);
        //    //other.gameObject.SetActive(false);
        //}
        //else if (other.tag == "Soda")
        //{
        //    //Heal(1);
        //    //other.gameObject.SetActive(false);
        //}
    }

    protected void Pickup (HoldableObject toPickup)
    {
        Debug.Log(toPickup);
        if (inventory.ContainsKey(toPickup.type))
        {
            List<HoldableObject> toPickupList;
            inventory.TryGetValue(toPickup.type, out toPickupList);
            toPickupList.Add(toPickup);
        }
        else
        {
            inventory.Add(toPickup.type, new List<HoldableObject>{toPickup});
        }
        toPickup.gameObject.SetActive(false);
    }

    protected void Attack (InteractableObject toAttack)
    {
        toAttack.TakeDamage(strength * (rationale / 50));

        animator.SetTrigger("playerChop");

        if (toAttack.health <= 0)
        {
            rationale -= (toAttack.reputation * 0.1);
            //rationaleText.text = "Rationale: " + rationale;
        }
    }

    protected void TalkTo (InteractableObject toTalkTo)
    {

    }

    protected IEnumerator postActionDelay()
    {
        yield return new WaitForSeconds(actionDelay);
    }

}
