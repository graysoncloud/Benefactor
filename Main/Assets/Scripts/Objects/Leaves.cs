using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaves : MonoBehaviour
{
    private int overlap;
    private float alpha;
    private float targetAlpha;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D boxCollider;

    void Start()
    {
        overlap = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        alpha = 1f;
        targetAlpha = 1f;
    }

    void Update() {
        if (alpha != targetAlpha) {
            alpha += 3f * Time.deltaTime * ((alpha < targetAlpha) ? 1 : -1);
            if (alpha > targetAlpha - 0.001f && alpha < targetAlpha + 0.001f)
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
        if (collision.gameObject.tag == "Character" || collision.gameObject.tag == "Indicator")
        {
            overlap++;
        }
        CheckOverlap();
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Character" || collision.gameObject.tag == "Indicator")
        {
            overlap--;
        }
        CheckOverlap();
    }

    void CheckOverlap()
    {
        targetAlpha = (overlap > 0) ? 0.25f : 1f;
    }

    void UpdateAlpha() {
        try {
            Color color = spriteRenderer.material.color;
            color.a = alpha;
            spriteRenderer.material.color = color; 
        } catch {}
    }
}
