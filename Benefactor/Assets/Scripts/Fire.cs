using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    protected BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        GameManager.instance.UpdateNode(transform.position, true, 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
