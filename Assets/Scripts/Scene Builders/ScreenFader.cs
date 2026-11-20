using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.3f;

    public void SetBlack() => group.alpha = 1f;

    public IEnumerator FadeOut() => Fade(0f, 1f);
    public IEnumerator FadeIn() => Fade(1f, 0f);

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}