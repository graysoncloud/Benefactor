using AStarSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    protected Vector2[] pathToObjective;
    protected Dictionary<Vector2, Vector2[]> paths;
    protected Dictionary<String, List<InteractableObject>> actableObjects;
    protected SortedSet<String> actions;
    protected Dictionary<String, List<HoldableObject>> inventory;
    protected int attackRange;
    protected List<InteractableObject> allies;
    protected bool destructive; //will destroy objects in path

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
        destructive = true; //make it start false unless agitated?

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        objectives = new Queue<Objective>();
        paths = new Dictionary<Vector2, Vector2[]>();
        actableObjects = new Dictionary<String, List<InteractableObject>>();
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
        FindPath();
        yield return new WaitForSeconds(moveTime);
        if (pathToObjective.Length > 0)
            yield return StartCoroutine(FollowPath());
        GetAvailableTargets();
        GetAvailableActions();
        Act();
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

    protected void LogPaths()
    {
        String actions = "Available Moves (" + paths.Count + "): ";
        foreach(KeyValuePair<Vector2, Vector2[]> entry in paths)
        {
            actions += entry.Key + ", ";
        }
        Debug.Log(actions);
    }

    virtual protected void FindPath()
    {
        Astar astar = new Astar(GameManager.instance.Grid);
        Stack<Node> path = astar.FindPath(transform.position, currentObjective.target.transform.position, destructive);
        int space = 1; //temporarily until adjust for targets you can stand on (interactable  vs holdable)
        pathToObjective = new Vector2[Math.Min(moves, path.Count - space)];
        int i = 0;
        foreach (Node node in path)
        {
            if (node.Weight > 1)
            {
                objectives.Enqueue(new Objective(currentObjective.target, currentObjective.action));
                boxCollider.enabled = false;
                Collider2D hitCollider = Physics2D.OverlapCircle(node.Position, 0.5f);
                boxCollider.enabled = true;
                currentObjective = new Objective(hitCollider.GetComponent<InteractableObject>(), "Attack");
                Array.Resize(ref pathToObjective, i);
                return;
            }

            pathToObjective[i] = node.Position;
            i++;
            if (i >= pathToObjective.Length)
                return;
        }
    }

    protected IEnumerator FollowPath()
    {
        isMoving = true;
        ErasePosition();
        foreach (Vector2 coords in pathToObjective)
        {
            yield return StartCoroutine(SmoothMovement(coords));
        }
        UpdatePosition();
        isMoving = false;
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
        actableObjects.Clear();

        GetAttackRange();
        if (inventory.ContainsKey("Weapon"))
            GetObjectsToActOn("Attack", attackRange);

        if (inventory.ContainsKey("Medicine"))
        {
            GetObjectsToActOn("Heal", 1);
            if (IsDamaged())
            {
                List<InteractableObject> objects;
                if (actableObjects.TryGetValue("Heal", out objects) == false)
                {
                    objects = new List<InteractableObject>();
                    actableObjects.Add("Heal", objects);
                }
                objects.Add(this);
            }
        }

        GetObjectsToActOn("Talk", 1);

        GetObjectsToActOn("Door", 1);

        GetObjectsToActOn("Unlock", 1);

        GetObjectsToActOn("Lever", 1);
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

    protected void GetObjectsToActOn(String action, int range)
    {
        List<InteractableObject>  objects = new List<InteractableObject>();

        boxCollider.enabled = false;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range);
        boxCollider.enabled = true;
        foreach (var hitCollider in hitColliders)
        {
            InteractableObject hitObject = hitCollider.GetComponent<InteractableObject>();
            if (hitObject != null && GetDistance(hitObject) <= range && GetDistance(hitObject) > 0 && hitObject.GetActions().Contains(action))
                objects.Add(hitObject);
        }

        if (objects.Count > 0)
            actableObjects.Add(action, objects);
    }

    virtual protected void GetAvailableActions()
    {
        actions.Clear();

        if (actableObjects.ContainsKey("Attack") && inventory.ContainsKey("Weapon"))
            actions.Add("Attack");

        if (actableObjects.ContainsKey("Heal") && inventory.ContainsKey("Medicine"))
            actions.Add("Heal");

        if (actableObjects.ContainsKey("Talk"))
            actions.Add("Talk");

        if (actableObjects.ContainsKey("Door"))
            actions.Add("Door");

        if (actableObjects.ContainsKey("Unlock") && inventory.ContainsKey("Key"))
            actions.Add("Unlock");

        if (actableObjects.ContainsKey("Lever"))
            actions.Add("Lever");

        actions.Add("Wait");
    }

    virtual protected void Act()
    {
        List<InteractableObject> objects;
        actableObjects.TryGetValue(currentObjective.action, out objects);

        switch (currentObjective.action)
        {
            case "Attack":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Weapon");
                else
                    StartCoroutine(EndTurn());
                break;
            case "Heal":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Medicine");
                else
                    StartCoroutine(EndTurn());
                break;
            case "Talk":
                if (objects != null && objects.Contains(currentObjective.target))
                    TalkTo(currentObjective.target);
                StartCoroutine(EndTurn());
                break;
            case "Door":
                if (objects != null && objects.Contains(currentObjective.target))
                    Toggle(currentObjective.target);
                StartCoroutine(EndTurn());
                break;
            case "Unlock":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Key");
                else
                    StartCoroutine(EndTurn());
                break;
            case "Lever":
                if (objects != null && objects.Contains(currentObjective.target))
                    Toggle(currentObjective.target);
                StartCoroutine(EndTurn());
                break;
            case "Wait":
                StartCoroutine(EndTurn());
                break;
            default:
                throw new Exception("Unknown action");
        }
    }

    protected virtual void SelectItem(String type)
    {
        List<HoldableObject> items;
        inventory.TryGetValue(type, out items);

        int i = 0;
        if (type == "Weapon")
            while (items[i].range < GetDistance(currentObjective.target))
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
            case "Key":
                Unlock(currentObjective.target, item);
                break;
            default:
                break;
        }

        StartCoroutine(EndTurn()); //TEMP?
    }

    protected virtual void Toggle(InteractableObject toToggle)
    {
        GameManager.instance.CameraTarget(toToggle.gameObject);

        Door door = toToggle.gameObject.GetComponent<Door>();
        if (door != null)
            door.Toggle();
        else
        {
            Lever lever = toToggle.gameObject.GetComponent<Lever>();
            lever.Toggle();
        }
    }

    protected virtual void Unlock(InteractableObject toUnlock, HoldableObject key)
    {
        GameManager.instance.CameraTarget(toUnlock.gameObject);

        Door door = toUnlock.gameObject.GetComponent<Door>();
        door.Unlock();

        Remove(key);
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
        GameManager.instance.CameraTarget(toAttack.gameObject);

        toAttack.TakeDamage((weapon.range == 1 ? strength : 1) * (rationale / 50) * weapon.amount);
        weapon.uses--;
        if (weapon.uses == 0)
            Remove(weapon);

        animator.SetTrigger("enemyAttack");

        if (toAttack.GetHealth() <= 0)
        {
            rationale -= (toAttack.GetReputation() * 0.1);
        }

        currentObjective = null; //TEMP
    }

    protected virtual void Heal(InteractableObject toHeal, HoldableObject medicine)
    {
        GameManager.instance.CameraTarget(toHeal.gameObject);

        toHeal.Heal(medicine.amount);

        Remove(medicine);

        currentObjective = null; //TEMP
    }

    protected void TalkTo(InteractableObject toTalkTo)
    {
        GameManager.instance.CameraTarget(toTalkTo.gameObject);
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
