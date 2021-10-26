using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roof : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float curOpacity;
    private float targetOpacity;
    private int roofIndex;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(targetOpacity - curOpacity) < 1)
            curOpacity = targetOpacity;
        else
        {
            if (curOpacity < targetOpacity)
                curOpacity++;
            else
                curOpacity--;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, curOpacity / 100);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.instance.IsPlayableCharacter(other.gameObject.GetComponent<Player>()))
        {
            hideRoof();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (GameManager.instance.IsPlayableCharacter(other.gameObject.GetComponent<Player>()))
        {
            showRoof();
        }
    }

    public void hideRoof()
    {
        if (!GetOverPlayer())
        {
            checkRoofs();
        }
    }

    public void showRoof()
    {
        if (GetOverPlayer())
        {
            checkRoofs();
        }
    }

    public void checkRoofs()
    {
        bool roofOverPlayer = false;
        foreach (Roof roof in GameManager.instance.Roofs[roofIndex]) {
            if (roof.GetOverPlayer())
                roofOverPlayer = true;
        }
        if (roofOverPlayer)
            HideRoofs();
        else
            ShowRoofs();
    }

    void HideRoofs()
    {
        foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
            roof.Hide();
    }

    void ShowRoofs()
    {
        foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
            roof.Show();
    }

    public void Hide()
    {
        targetOpacity = 0f;
    }

    public void Show()
    {
        targetOpacity = 100f;
    }

    public void setRoofIndex(int index)
    {
        roofIndex = index;
    }

    public bool GetOverPlayer()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapCircle((Vector2)transform.position, 0.1f);
        boxCollider.enabled = true;
        return hitCollider != null && GameManager.instance.IsPlayableCharacter(hitCollider.gameObject.GetComponent<Player>());
    }
}
