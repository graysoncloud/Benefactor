using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public LayerMask Collisions;

    public int health;
    public bool damageable;
    // Corpse refers to inanimate objects as well- a destroyed lever is a "corpse"
    public bool leavesCorpse;
    public bool isCorpse;

    public Sprite damagedSprite;
    public Sprite corpseSprite;
    private SpriteRenderer spriteRenderer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();

        isCorpse = false;
    }

    /**
     * Triggered when an NPC / event should damage the object.
     * Note- If need be, you could make a "takeTrueDamage" function to damage "undamageable" objects
     * 
     * @param damage How much health the action takes away
     */
    public void takeDamage(int damage)
    {
        if (!damageable) return;

        spriteRenderer.sprite = damagedSprite;
        health -= damage;

        if (health <= 0 && leavesCorpse)
        {
            spriteRenderer.sprite = corpseSprite;
            isCorpse = true;
        } else if (health <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
