using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : InteractableObject
{
    public Sprite front;
    public Sprite frontWindow;
    public Sprite frontWindowPlanter;
    public Sprite frontLeft;
    public Sprite frontRight;
    public Sprite frontUpDownLeft;
    public Sprite frontUpDownRight;
    public Sprite frontUp;
    public Sprite back;
    public Sprite backLeft;
    public Sprite backRight;
    public Sprite backUp;
    public Sprite backDown;
    public Sprite DownLeftBackRightFront;
    public Sprite DownLeftFrontRightBack;
    public Sprite backUpLeft;
    public Sprite backUpRight;
    public Sprite backUpDownLeft;
    public Sprite backUpDownRight;
    public Sprite backUpDownLeftRight;
    public Sprite UpDownLeftBackRightFront;
    public Sprite UpDownLeftFrontRightBack;
    public Sprite side;

    public GameObject edge;

    private int wallIndex;
    private bool isFront = false;
    private bool isDown = false;

    //Start is called before the first frame update
    void Start()
    {
        SetSprite();
        base.Start();
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void SetSprite()
    {
        bool isUp = false;
        isDown = false;
        bool isLeft = false;
        bool isRight = false;
        bool isRightFront = false;
        bool isLeftFront = false;
        bool nearWindow = false;
       
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
                Wall wall = hitCollider.GetComponent<Wall>();
                if (wall != null || hitCollider.GetComponent<Door>() != null) {
                    isLeft = true;
                    if (wall != null) {
                        isLeftFront = wall.GetInFront();
                        nearWindow = wall.IsWindow() ? true : nearWindow;
                    }
                }
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                Wall wall = hitCollider.GetComponent<Wall>();
                if (wall != null || hitCollider.GetComponent<Door>() != null) {
                    isRight = true;
                    if (wall != null) {
                        isRightFront = wall.GetInFront();
                        //nearWindow = wall.IsWindow() ? true : nearWindow;
                    }
                }
            }

        if (isUp && isDown && isLeft && isRight)
            spriteRenderer.sprite = isRightFront ? UpDownLeftBackRightFront : isLeftFront ? UpDownLeftFrontRightBack : backUpDownLeftRight;
        else if (isUp && isDown && isLeft)
            spriteRenderer.sprite = isLeftFront ? frontUpDownRight : backUpDownRight;
        else if (isUp && isDown && isRight)
            spriteRenderer.sprite = isRightFront ? frontUpDownLeft : backUpDownLeft;
        else if (isUp && isLeft && isRight)
            spriteRenderer.sprite = isFront ? frontUp : backUp;
        else if (isDown && isLeft && isRight)
            spriteRenderer.sprite = isRightFront ? DownLeftBackRightFront : isLeftFront ? DownLeftFrontRightBack : backDown;
        else if (isDown && isRight)
            spriteRenderer.sprite = backLeft;
        else if (isDown && isLeft)
            spriteRenderer.sprite = backRight;
        else if (isUp && isRight)
            spriteRenderer.sprite = isFront ? frontLeft : backUpLeft;
        else if (isUp && isLeft)
            spriteRenderer.sprite = isFront ? frontRight : backUpRight;
        else if (isRight && isLeft)
            spriteRenderer.sprite = isFront ? ((nearWindow || Random.Range(0,3) > 0) ? front : Random.Range(0,2) == 0 ? frontWindow : frontWindowPlanter) : back;
        else if (isUp && isDown)
            spriteRenderer.sprite = side;

        if (isDown)
            edge = Instantiate(edge, transform.position - new Vector3(0,1,0.1f), Quaternion.identity);
    }

    public void IsFront(bool inFront = true) {
        isFront = inFront;
    }

    public bool GetInFront() {
        return isFront;
    }

    public bool IsWindow() {
        return spriteRenderer.sprite == frontWindow || spriteRenderer.sprite == frontWindowPlanter;
    }

    protected override void ErasePosition()
    {
        if (isDown)
            GameObject.Destroy(edge.gameObject);
        base.ErasePosition();
    }
}