using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaves : MonoBehaviour
{
    private bool overlap;
    private float alpha;
    private float targetAlpha;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;
    protected Tree tree;

    void Start()
    {
        overlap = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        alpha = 1f;
        targetAlpha = 1f;
    }

    void Update() {
        if (alpha != targetAlpha) {
            alpha += 3f * Time.deltaTime * ((alpha < targetAlpha) ? 1 : -1);
            if (alpha > targetAlpha - 0.1f && alpha < targetAlpha + 0.1f)
                alpha = targetAlpha;
            UpdateAlpha();
        }
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Character")
        {
            overlap = true;
        }
        CheckOverlap();
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Character")
        {
            overlap = false;
        }
        CheckOverlap();
    }

    void CheckOverlap()
    {
        targetAlpha = (overlap) ? 0.25f : 1f;
    }

    void UpdateAlpha() {
        try {
            Color color = spriteRenderer.material.color;
            color.a = alpha;
            spriteRenderer.material.color = color;
            tree.GetComponent<SpriteRenderer>().material.color = color;
        } catch {}
    }

    public void SetTree(Tree newTree)
    {
        tree = newTree;
    }
}
