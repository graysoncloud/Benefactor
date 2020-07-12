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
            //transform.position = new Vector3(toFollow.position.x, toFollow.position.y, -10);
            Vector3 target = toFollow.position;
            target.z = -10;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }
    }
}
