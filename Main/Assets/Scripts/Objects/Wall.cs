using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : InteractableObject
{
    public Sprite front;
    public Sprite left;
    public Sprite right;
    public Sprite frontCornerLeft;
    public Sprite frontCornerRight;
    public Sprite backCornerLeft;
    public Sprite backCornerRight;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        SetSprite();
    }

    void SetSprite()
    {
        // bool isUp = false;
        // bool isDown = false;
        // bool isLeft = false;
        // bool isRight = false;
        // bool isDownLeft = false;
        // bool isDownRight = false;

        // foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
        // {
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(0, 1))
        //         isUp = true;
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(0, -1))
        //         isDown = true;
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(1, 0))
        //         isRight = true;
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(-1, 0))
        //         isLeft = true;
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(-1, -1))
        //         isDownLeft = true;
        //     if ((Vector2) roof.transform.position == ((Vector2) transform.position) + new Vector2(1, -1))
        //         isDownRight = true;
        // }

        // if (isDown && isUp && isLeft && isRight && !isDownLeft)
        //     spriteRenderer.sprite = frontInnerCornerLeft;
        // else if (isDown && isUp && isLeft && isRight && !isDownRight)
        //     spriteRenderer.sprite = frontInnerCornerRight;
        // else if (isDown && isLeft && isRight)
        //     spriteRenderer.sprite = flat;
        // else if (isUp && isLeft && isRight)
        //     spriteRenderer.sprite = front;
        // else if (isUp && isDown && isRight)
        //     spriteRenderer.sprite = left;
        // else if (isUp && isDown && isLeft)
        //     spriteRenderer.sprite = right;
        // else if (isUp && isRight)
        //     spriteRenderer.sprite = frontOuterCornerLeft;
        // else if (isUp && isLeft)
        //     spriteRenderer.sprite = frontOuterCornerRight;
        // else if (isDown && isRight)
        //     spriteRenderer.sprite = backCornerLeft;
        // else if (isDown && isLeft)
        //     spriteRenderer.sprite = backCornerRight;
    }
}