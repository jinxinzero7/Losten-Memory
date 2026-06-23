using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtPrompt : MonoBehaviour
{
    public float fadeDuration = 0.35f;
    public float floatDistance = 14f;
    public Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private Transform worldTarget;
    private Coroutine visibilityRoutine;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private bool isVisible;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        visiblePosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        hiddenPosition = visiblePosition + new Vector2(0f, -floatDistance);
        canvasGroup.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (worldTarget == null || rectTransform == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        if (canvasRectTransform == null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        Camera camera = Camera.main;
        if (camera == null || canvasRectTransform == null) return;

        Vector2 screenPoint = camera.WorldToScreenPoint(worldTarget.position + worldOffset);
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            visiblePosition = localPoint;
            hiddenPosition = visiblePosition + new Vector2(0f, -floatDistance);
            rectTransform.anchoredPosition = isVisible ? visiblePosition : hiddenPosition;
        }
    }

    public static ThoughtPrompt Ensure(GameObject target)
    {
        if (target == null) return null;

        ThoughtPrompt prompt = target.GetComponent<ThoughtPrompt>();
        if (prompt == null)
        {
            prompt = target.AddComponent<ThoughtPrompt>();
        }

        return prompt;
    }

    public static GameObject EnsurePrompt(GameObject target, string objectName, string text, Transform worldTarget, Vector3 offset, float width = 320f)
    {
        if (target == null)
        {
            target = CreatePromptObject(objectName);
        }

        ConfigureLabel(target, text, width);
        ThoughtPrompt prompt = Ensure(target);
        if (prompt != null)
        {
            prompt.AttachToWorld(worldTarget, offset);
        }

        target.SetActive(false);
        return target;
    }

    public void AttachToWorld(Transform target, Vector3 offset)
    {
        worldTarget = target;
        worldOffset = offset;
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (isVisible == visible && visibilityRoutine == null) return;

        isVisible = visible;
        gameObject.SetActive(true);

        if (visibilityRoutine != null)
        {
            StopCoroutine(visibilityRoutine);
        }

        visibilityRoutine = StartCoroutine(AnimateVisibility(visible));
    }

    private IEnumerator AnimateVisibility(bool visible)
    {
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = visible ? 1f : 0f;
        Vector2 startPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        Vector2 targetPosition = visible ? visiblePosition : hiddenPosition;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            t = t * t * (3f - 2f * t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            }

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPosition;
        }

        visibilityRoutine = null;
        if (!visible)
        {
            gameObject.SetActive(false);
        }
    }

    public static void ConfigureLabel(GameObject target, string text, float width = 300f)
    {
        if (target == null) return;

        TMP_Text label = target.GetComponent<TMP_Text>();
        if (label == null)
        {
            label = target.GetComponentInChildren<TMP_Text>(true);
        }

        if (label != null)
        {
            label.text = text;
            label.fontSize = 24f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
        }

        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 64f);
        }
    }

    private static GameObject CreatePromptObject(string objectName)
    {
        Canvas canvas = null;
        foreach (Canvas candidate in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
        {
            if (candidate.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = candidate;
                break;
            }
        }

        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Exclude);
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimePromptCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject promptObject = new GameObject(string.IsNullOrWhiteSpace(objectName) ? "InteractionPrompt" : objectName);
        promptObject.transform.SetParent(canvas.transform, false);
        promptObject.AddComponent<RectTransform>();
        promptObject.AddComponent<TextMeshProUGUI>();
        return promptObject;
    }
}
