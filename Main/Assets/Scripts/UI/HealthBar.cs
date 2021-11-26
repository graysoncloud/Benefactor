using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public float updateTime;
    public float hideDelay;
    private SpriteRenderer spriteRenderer;
    private float maxWidth;
    private float inverseUpdateTime;

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        maxWidth = spriteRenderer.size.x;
        spriteRenderer.enabled = false;
        inverseUpdateTime = 1 / updateTime;
    }

    public IEnumerator UpdateHealth(int currentHealth, int totalHealth, Vector3 position)
    {
        Debug.Log("UPDATE HEALTHBAR: " + currentHealth + " / " + totalHealth);
        spriteRenderer.enabled = true;
        float targetWidth = (float)currentHealth / (float)totalHealth;
        transform.localPosition = new Vector3(position.x - (maxWidth - spriteRenderer.size.x) / 2, position.y + 0.7f, position.y + 0.7f);
        float remaining = 1;
        try
        {
            remaining = spriteRenderer.size.x - targetWidth;
        }
        catch { yield break;  }
        Vector2 end = new Vector2(targetWidth, 0.2f);
        while (remaining > float.Epsilon)
        {
            Vector2 newSize = Vector2.MoveTowards(spriteRenderer.size, end, inverseUpdateTime * Time.deltaTime);
            remaining = spriteRenderer.size.x - targetWidth;
            spriteRenderer.size = newSize;
            transform.localPosition = new Vector3(position.x - (maxWidth - spriteRenderer.size.x) / 2, position.y + 0.7f, position.y + 0.7f);
            yield return null;
        }
        if (currentHealth > 0) 
            yield return new WaitForSeconds(hideDelay);
        spriteRenderer.enabled = false;
    }
}
