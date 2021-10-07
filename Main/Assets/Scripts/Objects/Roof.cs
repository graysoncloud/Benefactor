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
    private bool overPlayer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        overPlayer = false;
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
        if (other.gameObject.GetComponent<Player>() == GameManager.instance.activeCharacter)
        {
            overPlayer = true;
            checkRoofs();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == GameManager.instance.activeCharacter)
        {
            overPlayer = false;
            checkRoofs();
        }
    }

    void checkRoofs()
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
        return overPlayer;
    }
}
