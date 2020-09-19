﻿using AStarSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

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

    public class State
    {
        public double health;
        public int moves;
        public int actions;
        public Vector2 position;

        public State(double health, int moves, int actions, Vector2 position)
        {
            this.health = health;
            this.moves = moves;
            this.actions = actions;
            this.position = position;
        }
    }

    public float moveTime;
    public int totalMoves;
    public int talkingRange;
    public int totalActions;
    public float actionDelay;
    public double strength; //multiplier for range 1 weapon
    public double rationale;
    public bool isTurn;
    public bool isMoving;
    public bool talkable;
    public HoldableObject[] startingItems;

    protected Animator animator;
    private float inverseMoveTime;
    protected List<Objective> objectives;
    protected Objective currentObjective;
    protected Vector2[] pathToObjective;
    protected Dictionary<Vector2, Vector2[]> paths;
    protected Dictionary<String, List<InteractableObject>> actableObjects;
    protected SortedSet<String> actions;
    protected Dictionary<String, List<HoldableObject>> inventory;
    protected int attackRange;
    protected List<InteractableObject> allies;
    protected List<InteractableObject> enemies;
    protected bool destructive; //will destroy objects in path
    protected int movesLeft;
    protected int actionsLeft;
    protected State lastState;

    // Start is called before the first frame update
    protected override void Start()
    {
        isTurn = false;
        isMoving = false;
        destructive = true; //make it start false unless agitated?

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        objectives = new List<Objective>();
        paths = new Dictionary<Vector2, Vector2[]>();
        actableObjects = new Dictionary<String, List<InteractableObject>>();
        actions = new SortedSet<String>();
        inventory = new Dictionary<String, List<HoldableObject>>();
        allies = new List<InteractableObject>();
        allies.Add(this);
        enemies = new List<InteractableObject>();
        enemies.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<InteractableObject>()); //temporarily adds Player to enemies as default

        foreach (HoldableObject item in startingItems)
        {
            Pickup(Instantiate(item));
        }

        GameManager.instance.AddCharacterToList(this);

        base.Start();
    }

    public virtual void StartTurn()
    {
        isTurn = true;
        movesLeft = totalMoves;
        actionsLeft = totalActions;
        CheckSpace();
        UpdateState();
        StartCoroutine(NextStep());
    }

    protected virtual IEnumerator NextStep()
    {
        yield return new WaitForSeconds(actionDelay);
        GameManager.instance.CameraTarget(this.gameObject);
        //Debug.Log("Moves: " + movesLeft + ", Actions: " + actionsLeft);
        UpdateObjectives();
        LogObjectives();
        FindPath();
        yield return new WaitForSeconds(moveTime);
        if (pathToObjective.Length > 0)
        {
            yield return StartCoroutine(FollowPath());
            StartCoroutine(NextStep());
        }
        else
        {
            GetAvailableTargets();
            GetAvailableActions();
            Act();
         }
    }

    protected void LogObjectives()
    {
        Debug.Log("Current Objective: " + currentObjective.target + ": " + currentObjective.action);
        String actions = "Objectives Queue: ";
        foreach (Objective objective in objectives)
        {
            actions += objective.target + ": " + objective.action + ", ";
        }
        Debug.Log(actions);
    }

    protected virtual void UpdateObjectives()
    {
        Objective healClosest = new Objective(GetClosest(allies), "Heal");
        if (healClosest != null && IsDamaged() && inventory.ContainsKey("Medicine") && !HasObjective(healClosest))
            objectives.Prepend(healClosest);

        Objective attackClosest = new Objective(GetClosest(enemies), "Attack");
        if (attackClosest != null && destructive && !HasObjective(attackClosest))
            objectives.Add(attackClosest);

        if ((currentObjective == null && objectives.Count == 0) || (actionsLeft <= 0 && movesLeft <= 0))
        {
            if (currentObjective != null && currentObjective.action != "Wait")
                objectives.Prepend(new Objective(currentObjective.target, currentObjective.action));
            currentObjective = new Objective(this, "Wait");
        }

        if (currentObjective == null)
        {
            currentObjective = objectives[0];
            objectives.Remove(currentObjective);
        }
    }

    protected virtual bool HasObjective(Objective toCheck)
    {
        if (currentObjective != null && currentObjective.target == toCheck.target && currentObjective.action == toCheck.action) { return true; }
        foreach (Objective objective in objectives)
            if (objective.target == toCheck.target && objective.action == toCheck.action) { return true; }

        return false;
    }

    protected virtual InteractableObject GetClosest(List<InteractableObject> objects)
    {
        float minDistance = 99999;
        InteractableObject closest = null;

        foreach (InteractableObject o in objects)
        {
            float distance = GetDistance(o);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = o;
            }
        }

        return closest;
    }

    protected void LogPaths()
    {
        String actions = "Available Moves (" + paths.Count + "): ";
        foreach(KeyValuePair<Vector2, Vector2[]> entry in paths)
            actions += entry.Key + ", ";
        Debug.Log(actions);
    }

    virtual protected void FindPath()
    {
        Astar astar = new Astar(GameManager.instance.Grid);
        Stack<Node> path = astar.FindPath(transform.position, currentObjective.target.transform.position, destructive);
        int space = 1; //temporarily until adjust for targets you can stand on (interactable  vs holdable)
        pathToObjective = new Vector2[Math.Min(movesLeft, path.Count - space)];
        if (pathToObjective.Length == 0) { return; }
        int i = 0;
        foreach (Node node in path)
        {
            if (node.Weight > 1 || node.Weight == -1) //node.Weight == -1 signifies door
            {
                objectives.Prepend(new Objective(currentObjective.target, currentObjective.action));
                boxCollider.enabled = false;
                Collider2D hitCollider = Physics2D.OverlapCircle(node.Position, 0.5f);
                boxCollider.enabled = true;
                InteractableObject newTarget = hitCollider.GetComponent<InteractableObject>();
                currentObjective = new Objective(newTarget, newTarget.tag == "Door" ? "Door" : "Attack");
                Debug.Log("Obstacle: " + currentObjective.target + ": " + currentObjective.action);
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
            CheckSpace();
        }
        UpdatePosition();
        movesLeft -= pathToObjective.Length; //no "- 1" at end
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

        if (actionsLeft <= 0)
            return;

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

        GetObjectsToActOn("Talk", talkingRange);

        GetObjectsToActOn("Door", 1);

        if (inventory.ContainsKey("Key"))
            GetObjectsToActOn("Unlock", 1);

        GetObjectsToActOn("Lever", 1);

        GetObjectsToActOn("Loot", 1);
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
            {
                bool safe = true;
                if (hitObject.tag == "Door") //check if door is empty
                {
                    foreach(var hitCollider2 in hitColliders)
                    {
                        if (hitCollider != hitCollider2 && (Vector2)hitCollider.transform.position == (Vector2)hitCollider2.transform.position) {
                            safe = false;
                            break;
                        }
                    }
                }
                if (safe)
                    objects.Add(hitObject);
            }
        }

        if (objects.Count > 0)
            actableObjects.Add(action, objects);
    }

    virtual protected void GetAvailableActions()
    {
        actions.Clear();

        foreach (String action in actableObjects.Keys)
            actions.Add(action);

        actions.Add("Wait");
    }

    virtual protected void Act()
    {
        actionsLeft--;
        UpdateState();
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
                StartCoroutine(NextStep());
                currentObjective = null;
                break;
            case "Door":
                if (objects != null && objects.Contains(currentObjective.target))
                    Toggle(currentObjective.target);
                StartCoroutine(NextStep());
                currentObjective = null;
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
                StartCoroutine(NextStep());
                currentObjective = null;
                break;
            case "Loot":
                if (objects != null && objects.Contains(currentObjective.target))
                    Loot(currentObjective.target);
                break;
            case "Wait":
                StartCoroutine(EndTurn());
                currentObjective = null;
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
                StartCoroutine(NextStep());
                break;
            case "Medicine":
                Heal(currentObjective.target, item);
                StartCoroutine(NextStep());
                break;
            case "Key":
                Unlock(currentObjective.target, item);
                break;
            default:
                break;
        }
        currentObjective = null;
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
        if (door != null)
        {
            door.Unlock();
            StartCoroutine(NextStep());
        }
        else
        {
            Storage storage = toUnlock.gameObject.GetComponent<Storage>();
            storage.Unlock();
            Loot(toUnlock);
        }

        Remove(key);
    }

    protected virtual void Loot(InteractableObject toLoot)
    {
        GameManager.instance.CameraTarget(toLoot.gameObject);

        Storage storage = toLoot.gameObject.GetComponent<Storage>();
        storage.Open();

        foreach (HoldableObject item in storage.items)
        {
            Pickup(item);
            storage.Remove(item);
        }

        storage.Close();

        StartCoroutine(NextStep());
        currentObjective = null;
    }

    protected virtual IEnumerator EndTurn()
    {
        //yield return new WaitForSeconds(actionDelay);

        // Conditionally doesn't end your turn if there is still dialogue in progress
        if (GameManager.instance.dialogueInProgress)
            yield return new WaitUntil(() => GameManager.instance.dialogueInProgress == false);
        else
            yield return new WaitForSeconds(0f);

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

        UpdateState();
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

        toAttack.TakeDamage(weapon.amount * (weapon.range == 1 ? strength : 1) * (rationale / 50));
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

        toHeal.Heal(medicine.amount * (rationale / 50));

        Remove(medicine);

        currentObjective = null; //TEMP
    }

    protected void TalkTo(InteractableObject toTalkTo)
    {
        GameObject.Find("DialogueManager").GetComponent<DialogueManager>().initiateDialogue(this.gameObject, toTalkTo.gameObject);
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (talkable)
            receiveActions.Add("Talk");

        return receiveActions;
    }

    protected void CheckSpace()
    {
        boxCollider.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapCircle((Vector2)transform.position, 0.1f);
        boxCollider.enabled = true;
        if (hitCollider != null && hitCollider.gameObject.tag == "Damaging")
            TakeDamage(1);
    }

    protected void UpdateState()
    {
        lastState = new State(health, movesLeft, actionsLeft, (Vector2)transform.position);
    }

    protected IEnumerator postActionDelay()
    {
        yield return new WaitForSeconds(actionDelay);
    }

}
