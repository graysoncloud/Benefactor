using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Character : InteractableObject
{
    public class Objective
    {
        public InteractableObject target;
        public string action;

        public Objective(InteractableObject target, string action)
        {
            this.target = target;
            this.action = action;
        }
    }

    public float moveTime;
    public int moves;
    public float actionDelay;
    public double strength; //multiplier for range 1 weapon
    public bool isTurn;
    public bool isMoving;
    public bool talkable;
    public HoldableObject[] startingItems;

    protected double rationale;
    protected Animator animator;
    private float inverseMoveTime;
    protected Queue<Objective> objectives;
    protected Objective currentObjective;
    //protected Vector2[] pathToObjective;
    protected Dictionary<Vector2, Vector2[]> paths;
    protected Vector2 toMove;
    protected List<InteractableObject> healableObjects;
    protected List<InteractableObject> talkableObjects;
    protected List<InteractableObject> attackableObjects;
    protected SortedSet<String> actions;
    protected Dictionary<String, List<HoldableObject>> inventory;
    protected int attackRange;

    // Start is called before the first frame update
    protected override void Start()
    {
        rationale = GameManager.instance.defaultRationale;
        moves = GameManager.instance.defaultMoves;
        strength = GameManager.instance.defaultStrength;
        moveTime = GameManager.instance.defaultMoveTime;
        actionDelay = GameManager.instance.defaultActionDelay;

        isTurn = false;
        isMoving = false;

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        objectives = new Queue<Objective>();
        paths = new Dictionary<Vector2, Vector2[]>();
        healableObjects = new List<InteractableObject>();
        talkableObjects = new List<InteractableObject>();
        attackableObjects = new List<InteractableObject>();
        actions = new SortedSet<String>();
        inventory = new Dictionary<String, List<HoldableObject>>();
        foreach (HoldableObject item in startingItems)
        {
            Pickup(Instantiate(item));
        }

        GameManager.instance.AddCharacterToList(this);

        base.Start();
    }

    public virtual IEnumerator StartTurn()
    {
        isTurn = true;
        UpdateObjectives();
        GetPaths();
        yield return new WaitForSeconds(moveTime);
        SelectPath();
        yield return StartCoroutine(FollowPath());
        GetAvailableTargets();
        GetAvailableActions();
        Act();
        StartCoroutine(EndTurn());
    }

    protected virtual void UpdateObjectives()
    {
        if (IsDamaged() && inventory.ContainsKey("Medicine") && !objectives.Contains(new Objective(this, "Heal")))
            objectives.Enqueue(new Objective(this, "Heal"));
        else if (!objectives.Contains(new Objective(GameObject.FindGameObjectWithTag("Player").GetComponent<InteractableObject>(), "Attack")))
            objectives.Enqueue(new Objective(GameObject.FindGameObjectWithTag("Player").GetComponent<InteractableObject>(), "Attack"));
        if (currentObjective == null)
            currentObjective = objectives.Dequeue();
    }

    protected void GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], moves);
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

    virtual protected void SelectPath()
    {
        //might update to do A* search eventually to find shortest path
        int minDistance = 999;
        foreach (KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            int distance = (int)(Math.Abs(entry.Key.x - currentObjective.target.transform.position.x) + Math.Abs(entry.Key.y - currentObjective.target.transform.position.y));
            if (distance < minDistance)
            {
                toMove = entry.Key;
                minDistance = distance;
            }
        }
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
                //movesUsed++;
                StartCoroutine(SmoothMovement(coords));
                yield return new WaitForSeconds(moveTime);
            }
            isMoving = false;
        } else
        {
            yield return new WaitForSeconds(moveTime);
        }
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

    virtual protected void GetAvailableTargets()
    {
        GetAttackRange();
        attackableObjects.Clear();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        if (inventory.ContainsKey("Weapon"))
            GetObjectsToActOn(attackableObjects, "Attack", transform.position, visited, attackRange);

        healableObjects.Clear();
        visited.Clear();
        if (inventory.ContainsKey("Medicine"))
        {
            GetObjectsToActOn(healableObjects, "Heal", transform.position, visited, 1);
            if (IsDamaged())
                healableObjects.Add(this);
        }

        talkableObjects.Clear();
        visited.Clear();
        GetObjectsToActOn(talkableObjects, "Talk", transform.position, visited, 1);
    }

    protected void GetAttackRange()
    {
        attackRange = 1;
        if (inventory.ContainsKey("Weapon"))
        {
            List<HoldableObject> weapons;
            inventory.TryGetValue("Weapon", out weapons);
            foreach (HoldableObject weapon in weapons)
            {
                if (weapon.range > attackRange)
                    attackRange = weapon.range;
            }
        }
    }

    protected void GetObjectsToActOn(List<InteractableObject> objects, String action, Vector2 toCheck, HashSet<Vector2> visited, int range)
    {
        if (visited.Contains(toCheck))
            return;
        visited.Add(toCheck);

        boxCollider.enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, toCheck, Collisions);
        if (hit.collider != null)
        {
            InteractableObject hitObject = hit.transform.GetComponent<InteractableObject>();
            if (hitObject != null && hitObject.GetActions().Contains(action))
            {
                objects.Add(hitObject);
            }
        }
        boxCollider.enabled = true;

        if (range > 0)
        {
            GetObjectsToActOn(objects, action, toCheck + new Vector2(1, 0), visited, range - 1);
            GetObjectsToActOn(objects, action, toCheck + new Vector2(-1, 0), visited, range - 1);
            GetObjectsToActOn(objects, action, toCheck + new Vector2(0, 1), visited, range - 1);
            GetObjectsToActOn(objects, action, toCheck + new Vector2(0, -1), visited, range - 1);
        }
    }

    virtual protected void GetAvailableActions()
    {
        actions.Clear();

        if (attackableObjects.Count > 0)
            actions.Add("Attack");

        if (healableObjects.Count > 0)
            actions.Add("Heal");

        if (talkableObjects.Count > 0)
            actions.Add("Talk");

        actions.Add("Wait");
    }

    virtual protected void Act()
    {
        switch (currentObjective.action)
        {
            case "Attack":
                if (attackableObjects.Contains(currentObjective.target))
                    SelectItem("Weapon");
                break;
            case "Heal":
                if (healableObjects.Contains(currentObjective.target))
                    SelectItem("Medicine");
                break;
            case "Talk":
                if (talkableObjects.Contains(currentObjective.target))
                    TalkTo(currentObjective.target);
                StartCoroutine(EndTurn());
                break;
            case "Wait":
                StartCoroutine(EndTurn());
                break;
            default:
                StartCoroutine(EndTurn());
                break;
        }
    }

    protected virtual void SelectItem(String type)
    {
        List<HoldableObject> items;
        inventory.TryGetValue(type, out items);

        int i = 0;
        if (type == "Weapon")
            while (items[i].range < GetDistance(currentObjective.target) - 1)
                i++;
        ChooseItem(items[i]);
    }

    public virtual void ChooseItem(HoldableObject item)
    {
        switch (item.type)
        {
            case "Weapon":
                Attack(currentObjective.target, item);
                break;
            case "Medicine":
                Heal(currentObjective.target, item);
                break;
            default:
                break;
        }
    }

    protected IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(actionDelay);
        isTurn = false;
        StartCoroutine(GameManager.instance.nextTurn());
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<HoldableObject>() != null)
        {
            Pickup(other.gameObject.GetComponent<HoldableObject>());
        }
    }

    protected virtual void Pickup (HoldableObject toPickup, Boolean start = false)
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
        if (!start)
            toPickup.gameObject.SetActive(false);
    }

    protected void Remove(HoldableObject item)
    {
        List<HoldableObject> items;
        inventory.TryGetValue(item.type, out items);
        items.Remove(item);
        if (items.Count == 0)
            inventory.Remove(item.type);
    }

    protected void Attack (InteractableObject toAttack, HoldableObject weapon)
    {
        toAttack.TakeDamage((weapon.range == 1 ? strength : 1) * (rationale / 50) * weapon.amount);
        weapon.uses--;
        if (weapon.uses == 0)
            Remove(weapon);

        animator.SetTrigger("enemyAttack");

        if (toAttack.GetHealth() <= 0)
        {
            rationale -= (toAttack.GetReputation() * 0.1);
        }
    }

    protected virtual void Heal(InteractableObject toHeal, HoldableObject medicine)
    {
        toHeal.Heal(medicine.amount);

        Remove(medicine);
    }

    protected void TalkTo(InteractableObject toTalkTo)
    {

    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (talkable)
            receiveActions.Add("Talk");

        return receiveActions;
    }

    protected IEnumerator postActionDelay()
    {
        yield return new WaitForSeconds(actionDelay);
    }

}
