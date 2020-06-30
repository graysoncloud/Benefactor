using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Sprite dmgSprite;
    public int HP = 4;

    private SpriteRenderer spriteRenderer;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall (int loss)
    {
        spriteRenderer.sprite = dmgSprite;
        HP -= loss;
        if (HP <= 0)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
