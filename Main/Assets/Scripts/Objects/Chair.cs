using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : InteractableObject
{
    public Sprite front;
    public Sprite back;
    public Sprite left;
    public Sprite right;
    private float offset;

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
                if (hitCollider.GetComponent<InteractableObject>() != null && hitCollider.gameObject.tag == "Table")
                    isUp = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, -1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null && hitCollider.gameObject.tag == "Table")
                    isDown = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(-1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null && hitCollider.gameObject.tag == "Table")
                    isLeft = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null && hitCollider.gameObject.tag == "Table")
                    isRight = true;
            }
        
        if (!isUp && !isDown && !isLeft && !isRight) {
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, 1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null)
                    isDown = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(0, -1), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null)
                    isUp = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(-1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null)
                    isRight = true;
            }
            hitColliders = Physics2D.OverlapCircleAll(((Vector2) transform.position) + new Vector2(1, 0), 0.5f);
            foreach (Collider2D hitCollider in hitColliders) {
                if (hitCollider.GetComponent<InteractableObject>() != null)
                    isLeft = true;
            }
        }

        if (isUp) {
            spriteRenderer.sprite = back;
            offset = -1;
        }
        else if (isDown) {
            spriteRenderer.sprite = front;
            offset = 1;
        }
        else if (isRight) {
            spriteRenderer.sprite = left;
            offset = -1;
        }
        else if (isLeft) {
            spriteRenderer.sprite = right;
            offset = -1;
        }
    }

    protected override void UpdatePosition()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + offset);
        base.UpdatePosition();
    }
}
