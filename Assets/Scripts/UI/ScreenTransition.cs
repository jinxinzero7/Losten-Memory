using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    private static ScreenTransition instance;
    private CanvasGroup overlay;
    private bool isTransitioning;

    public static bool IsTransitioning => instance != null && instance.isTransitioning;

    public static void LoadSceneWithFade(string sceneName, Action beforeLoad = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        EnsureInstance().StartCoroutine(EnsureInstance().FadeAndLoad(sceneName, beforeLoad));
    }

    private static ScreenTransition EnsureInstance()
    {
        if (instance != null) return instance;

        GameObject transitionObject = new GameObject("ScreenTransition");
        instance = transitionObject.AddComponent<ScreenTransition>();
        DontDestroyOnLoad(transitionObject);
        instance.BuildOverlay();
        return instance;
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();
        overlay = gameObject.AddComponent<CanvasGroup>();
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Fade");
        imageObject.transform.SetParent(transform, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeAndLoad(string sceneName, Action beforeLoad)
    {
        if (isTransitioning) yield break;

        isTransitioning = true;
        overlay.blocksRaycasts = true;
        yield return FadeTo(1f, 0.35f);
        yield return new WaitForSecondsRealtime(0.12f);

        beforeLoad?.Invoke();
        SceneManager.LoadScene(sceneName);

        yield return null;
        yield return new WaitForSecondsRealtime(0.12f);
        yield return FadeTo(0f, 0.35f);
        overlay.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = overlay.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        overlay.alpha = targetAlpha;
    }
}
