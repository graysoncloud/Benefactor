using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSprite : MonoBehaviour
{
    public float speed = 90;
    public float openAngle = 90;

    private Vector2 initialPos;
    private Vector3 curAngle;
    private float startAngle;
    private float targetAngle;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    public Sprite unlockedSprite;

    // Start is called before the first frame update
    protected void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        curAngle = transform.eulerAngles;
        startAngle = curAngle.z;
        targetAngle = startAngle;
        initialPos = new Vector2(transform.position.x - 0.40625f, transform.position.y - 0.40625f);
        rb2D.MovePosition(initialPos);
    }

    void Update()
    {
        curAngle.z = Mathf.MoveTowards(curAngle.z, targetAngle, speed * Time.deltaTime);
        transform.eulerAngles = curAngle;
    }

    public void Open()
    {
        targetAngle = startAngle + openAngle;
    }
    
    public void Close()
    {
        targetAngle = startAngle;
    }

    public void Unlock()
    {
        spriteRenderer.sprite = unlockedSprite;
    }
}
