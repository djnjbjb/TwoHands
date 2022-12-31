using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControlFade
{
    float alpha = 1f;
    readonly SpriteRenderer[] spriteRenderers;

    public HandControlFade(GameObject whole)
    {
        spriteRenderers = whole.transform.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
    }

    public void Fade(MonoBehaviour mono, float fadeTime)
    {
        mono.StartCoroutine(FadeCoroutine(fadeTime));
    }

    IEnumerator FadeCoroutine(float fadeTime)
    {
        float startTime = Time.time;
        while (Time.time - startTime < fadeTime)
        {
            alpha = 1 - (Time.time - startTime) / fadeTime;
            foreach (var render in spriteRenderers)
            {
                render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
            }
            yield return null;
        }

        alpha = 0f;
        foreach (var render in spriteRenderers)
        {
            render.color = new Color(render.color.r, render.color.g, render.color.b, alpha);
        }
    }
}
