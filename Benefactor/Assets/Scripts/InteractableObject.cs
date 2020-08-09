using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public LayerMask Collisions;
    public double maxHealth;
    public bool damageable;
    public bool repairable;
    public bool leavesCorpse; // Corpse refers to inanimate objects as well- a destroyed lever is a "corpse"
    public Sprite damagedSprite;
    public Sprite corpseSprite;
    private SpriteRenderer spriteRenderer;

    protected double health;
    protected bool isCorpse;
    protected int reputation;
    protected SortedSet<String> receiveActions;
    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rb2D;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        reputation = GameManager.instance.defaultReputation;
        health = maxHealth;
        //leavesCorpse = false;
        isCorpse = false;
        receiveActions = new SortedSet<String>();
    }

    /**
     * Triggered when an NPC / event should damage the object.
     * Note- If need be, you could make a "takeTrueDamage" function to damage "undamageable" objects
     * 
     * @param damage How much health the action takes away
     */
    public virtual void TakeDamage(double damage)
    {
        if (!damageable) return;

        spriteRenderer.sprite = damagedSprite;
        health = Math.Max(health - damage, 0);

        if (health <= 0 && leavesCorpse)
        {
            spriteRenderer.sprite = corpseSprite;
            isCorpse = true;
        } else if (health <= 0)
        {
            gameObject.SetActive(false);
            GameManager.instance.RemoveDeadCharacters();
            // For some reason, this didn't work, so instead, GameManager just doesn't move characters at <= 0 health
        }
    }

    public virtual void Heal(int amount)
    {
        if (!damageable) return;
        health = Math.Min(health + amount, maxHealth);
    }

    public virtual double GetHealth()
    {
        return health;
    }

    public virtual bool IsDamaged()
    {
        return health < maxHealth;
    }

    public virtual int GetReputation()
    {
        return reputation;
    }

    public virtual SortedSet<String> GetActions()
    {
        receiveActions.Clear();

        if (damageable)
            receiveActions.Add("Attack");

        if (repairable && IsDamaged())
            receiveActions.Add("Heal");

        return receiveActions;
    }

    public virtual int GetDistance(InteractableObject o)
    {
        return (int)((o.transform.position.x - transform.position.x) + (o.transform.position.y - transform.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}