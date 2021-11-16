using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : InteractableObject
{
    public Sprite front;
    public Sprite left;
    public Sprite right;
    public Sprite frontLeft;
    public Sprite frontRight;
    public Sprite backLeft;
    public Sprite backRight;
    public Sprite cornerLeft;
    public Sprite cornerRight;

    private int wallIndex;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        SetSprite();
    }

    void SetSprite()
    {
        bool isSame = false;
        bool isUp = false;
        bool isDown = false;
        bool isLeft = false;
        bool isRight = false;
        // bool isUpLeft = false;
        // bool isUpRight = false;
        bool isDownLeft = false;
        bool isDownRight = false;
        bool isDownDownLeft = false;
        bool isDownDownRight = false;

        // foreach (GameObject floor in GameManager.instance.Floors[wallIndex])
        // {
        //     if ((Vector2) floor.transform.position == (Vector2) transform.position)
        //         isSame = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(0, 1))
        //         isUp = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(0, -1))
        //         isDown = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(1, 0))
        //         isRight = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(-1, 0))
        //         isLeft = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(-1, 1))
        //         isUpLeft = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(1, 1))
        //         isUpRight = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(-1, -1))
        //         isDownLeft = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(1, -1))
        //         isDownRight = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(-1, -2))
        //         isDownDownLeft = true;
        //     if ((Vector2) floor.transform.position == ((Vector2) transform.position) + new Vector2(1, -2))
        //         isDownDownRight = true;
        // }

        if (isUp && isDown)
            spriteRenderer.sprite = front;
        else if (isSame && isUp && isRight && isDownRight)
            spriteRenderer.sprite = cornerLeft;
        else if (isSame && isUp && isLeft && isDownLeft)
            spriteRenderer.sprite = cornerRight;
        else if ((isUp && isSame) || isDown)
            spriteRenderer.sprite = front;
        else if (isRight && !isDownRight && !isDownDownRight)
            spriteRenderer.sprite = frontLeft;
        else if (isLeft && !isDownLeft && !isDownDownLeft)
            spriteRenderer.sprite = frontRight;
        else if (isRight)
            spriteRenderer.sprite = left;
        else if (isLeft)
            spriteRenderer.sprite = right;
        else if (isDownRight)
            spriteRenderer.sprite = backLeft;
        else if (isDownLeft)
            spriteRenderer.sprite = backRight;
    }

    public void setWallIndex(int index) {
        wallIndex = index;
    }
}