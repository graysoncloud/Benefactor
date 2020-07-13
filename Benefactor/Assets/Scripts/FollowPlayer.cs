using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public float moveSpeed;
    public Transform toFollow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.activeCharacter != null)
        {
            toFollow = GameManager.instance.activeCharacter.transform;
            Vector3 target = toFollow.position;
            target.z = -10;
            float distance = System.Math.Abs(transform.position.x - target.x) + System.Math.Abs(transform.position.y - target.y);
            if (distance < 0.1)
            {
                transform.position = new Vector3(target.x, target.y, -10);
        }
        else
        {
            moveSpeed = distance * 2;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }
    }
    }
}
