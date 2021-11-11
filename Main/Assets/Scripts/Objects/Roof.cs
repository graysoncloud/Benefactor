using System;
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
        boxCollider = GetComponent<BoxCollider2D>();
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
                curOpacity += 200 * Time.deltaTime;
            else
                curOpacity -= 200 * Time.deltaTime;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, curOpacity / 100);
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log("PLAYER ENTER");
    //     Player player = other.gameObject.GetComponent<Player>();
    //     Debug.Log(player);
    //     Debug.Log(player.playable);
    //     if (player != null && player.playable)
    //     {
    //         hideRoof();
    //     }
    // }

    // void OnTriggerExit2D(Collider2D other)
    // {
    //     Debug.Log("PLAYER EXIT");
    //     Player player = other.gameObject.GetComponent<Player>();
    //     Debug.Log(player);
    //     Debug.Log(player.playable);
    //     if (player != null && player.playable)
    //     {
    //         showRoof();
    //     }
    // }

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
        Debug.Log(GameManager.instance.Roofs);
        foreach (Roof roof in GameManager.instance.Roofs[roofIndex])
        {
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

    public bool Hidden()
    {
        return targetOpacity == 0f;
    }

    public void setRoofIndex(int index)
    {
        roofIndex = index;
    }

    public bool GetOverPlayer()
    {
        foreach (Player player in GameManager.instance.characters) {
            if (player.playable && (Vector2)transform.position == (Vector2)player.transform.position) {
                return true;
            }
        }
        return false;
    }
}
