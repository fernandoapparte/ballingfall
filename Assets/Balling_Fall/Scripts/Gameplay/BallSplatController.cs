using System.Collections;
using UnityEngine;

public class BallSplatController : MonoBehaviour {

    private SpriteRenderer spRender = null;
    public void FadeOut(Color color, float fadingTime)
    {
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        spRender.color = color;
        StartCoroutine(FadingOut(fadingTime));
    }
    private IEnumerator FadingOut(float fadingTime)
    {
        Color startColor = spRender.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        float t = 0;
        while (t < fadingTime)
        {
            t += Time.deltaTime;
            float factor = t / fadingTime;
            spRender.color = Color.Lerp(startColor, endColor, factor);
            yield return null;
        }
        gameObject.SetActive(false);
    }

}
