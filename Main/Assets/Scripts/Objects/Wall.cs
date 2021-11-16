using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : InteractableObject
{
    public Sprite front;
    public Sprite frontLeft;
    public Sprite frontRight;
    public Sprite frontUp;
    public Sprite back;
    public Sprite backLeft;
    public Sprite backRight;
    public Sprite backUp;
    public Sprite backDown;
    public Sprite backUpLeft;
    public Sprite backUpRight;
    public Sprite backUpDownLeft;
    public Sprite backUpDownRight;
    public Sprite backUpDownLeftRight;
    public Sprite side;

    private int wallIndex;
    private bool isFront = false;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        SetSprite();
    }

    void SetSprite()
    {
        bool isUp = false;
        bool isDown = false;
        bool isLeft = false;
        bool isRight = false;
       
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, 1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null || hitCollider.GetComponent<Door>() != null)
                    isUp = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, -1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null || hitCollider.GetComponent<Door>() != null)
                    isDown = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(-1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null || hitCollider.GetComponent<Door>() != null)
                    isLeft = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<Wall>() != null || hitCollider.GetComponent<Door>() != null)
                    isRight = true;
            }

        if (isUp && isDown && isLeft && isRight)
            spriteRenderer.sprite = backUpDownLeftRight;
        else if (isUp && isDown && isLeft)
            spriteRenderer.sprite = backUpDownRight;
        else if (isUp && isDown && isRight)
            spriteRenderer.sprite = backUpDownLeft;
        else if (isUp && isLeft && isRight)
            spriteRenderer.sprite = isFront ? frontUp : backUp;
        else if (isDown && isLeft && isRight)
            spriteRenderer.sprite = backDown;
        else if (isDown && isRight)
            spriteRenderer.sprite = backLeft;
        else if (isDown && isLeft)
            spriteRenderer.sprite = backRight;
        else if (isUp && isRight)
            spriteRenderer.sprite = isFront ? frontLeft : backUpLeft;
        else if (isUp && isLeft)
            spriteRenderer.sprite = isFront ? frontRight : backUpRight;
        else if (isRight && isLeft)
            spriteRenderer.sprite = isFront ? front : back;
        else if (isUp && isDown)
            spriteRenderer.sprite = side;
    }

    public void IsFront(bool inFront = true) {
        isFront = inFront;
    }
}