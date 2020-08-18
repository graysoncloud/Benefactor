using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSprite : MonoBehaviour
{
    public float speed = 90; // door speed in degrees per second
    public float openAngle = 90; // opening angle in degrees

    private Vector2 initialPos;
    private Vector3 curAngle;
    private float startAngle;
    private float targetAngle;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    protected void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        curAngle = transform.eulerAngles;
        startAngle = curAngle.z; // save the startAngle
        targetAngle = startAngle;
        initialPos = new Vector2(transform.position.x - 0.40625f, transform.position.y - 0.40625f);
        rb2D.MovePosition(initialPos);
    }

    void Update()
    { // rotate gradually door to targetAngle, if different of curAngle:
        curAngle.z = Mathf.MoveTowards(curAngle.z, targetAngle, speed * Time.deltaTime);
        transform.eulerAngles = curAngle;
        //float xDisplacement = (float)Math.Cos(curAngle.z * Math.PI / 180)/2;
        //float yDisplacement = (float)Math.Sin(curAngle.z * Math.PI / 180)/2;
        //Vector2 newPosition = new Vector2(initialPos.x + xDisplacement, initialPos.y + yDisplacement);
        //rb2D.MovePosition(newPosition);
    }

    public void Open() //thanks to https://answers.unity.com/questions/239912/rotate-door-object-away-from-player.html
    {
        targetAngle = startAngle + openAngle;
    }
    
    public void Close()
    {
        targetAngle = startAngle;
    }
}
