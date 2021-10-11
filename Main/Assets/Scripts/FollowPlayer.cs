using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    private float moveSpeed;
    public bool followMouse;
    protected GameObject toFollow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followMouse || toFollow != null) {
            Vector3 target = new Vector3();
            if (followMouse)
            {
                Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                double moveFraction = 0.1; //only move camera if the mouse is this close to edge of screen
                if (mouseScreenPosition.x > Screen.width * moveFraction && mouseScreenPosition.x < Screen.width * (1-moveFraction) &&
                    mouseScreenPosition.y > Screen.height * moveFraction && mouseScreenPosition.y < Screen.height * (1-moveFraction))
                {
                    return;
                }

                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                target = mouseWorldPosition;
            }
            else
                target = toFollow.transform.position;
            target.z = -10;
            float distance = Math.Abs(transform.position.x - target.x) + Math.Abs(transform.position.y - target.y);
            if (distance < 0.03)
            {
                transform.position = new Vector3(target.x, target.y, -10);
            }
            else
            {
                moveSpeed = Math.Max(distance * 2, 0.03f);
                Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                Vector2 minPosition = new Vector2(7.0f, 3.0f);
                BoardManager board = GameManager.instance.GetComponent<BoardManager>();
                Vector2 maxPosition = new Vector2((float)board.columns - minPosition.x - 1, (float)board.rows - minPosition.y - 1);
                if (newPosition.x < minPosition.x)
                    newPosition.x = minPosition.x;
                if (newPosition.y < minPosition.y)
                    newPosition.y = minPosition.y;
                if (newPosition.x > maxPosition.x)
                    newPosition.x = maxPosition.x;
                if (newPosition.y > maxPosition.y)
                    newPosition.y = maxPosition.y;
                transform.position = newPosition;
            }
        }
    }

    public void Target(GameObject newTarget)
    {
        toFollow = newTarget;
    }

    public void FollowMouse()
    {
        followMouse = true;
    }

    public void FollowTarget()
    {
        followMouse = false;
    }

}
