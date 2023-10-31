using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PageAnimator
{
    public static IEnumerator SlideIn(
        RectTransform rectTransform,
        CanvasGroup canvasGroup,
        Direction direction, 
        float duration, 
        Action OnEnd)
    {
        Vector2 targetPosition = Vector2.zero;
        Vector2 startPosition = Vector2.zero;

        var cavnasScaler = rectTransform.GetComponentInParent<CanvasScaler>();
        
        switch (direction)
        {
            case Direction.Left:
                startPosition.x += cavnasScaler.referenceResolution.x;
                break;
            case Direction.Right:
                startPosition.x -= cavnasScaler.referenceResolution.x;
                break;
            case Direction.Up:
                startPosition.y -= cavnasScaler.referenceResolution.y;
                break;
            case Direction.Down:
                startPosition.y += cavnasScaler.referenceResolution.y;
                break;
        }

        rectTransform.anchoredPosition = startPosition;
        canvasGroup.interactable = false;

        float time = 0f;
        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.anchoredPosition = targetPosition;
        canvasGroup.interactable = true;

        OnEnd?.Invoke();
    }

    public static IEnumerator SlideOut(
        RectTransform rectTransform,
        CanvasGroup canvasGroup,
        Direction direction,
        float duration,
        Action OnEnd)
    {
        Vector2 targetPosition = Vector2.zero;
        Vector2 startPosition = Vector2.zero;

        var cavnasScaler = rectTransform.GetComponentInParent<CanvasScaler>();

        switch (direction)
        {
            case Direction.Left:
                targetPosition.x -= cavnasScaler.referenceResolution.x;
                break;
            case Direction.Right:
                targetPosition.x += cavnasScaler.referenceResolution.x;
                break;
            case Direction.Up:
                targetPosition.y += cavnasScaler.referenceResolution.y;
                break;
            case Direction.Down:
                targetPosition.y -= cavnasScaler.referenceResolution.y;
                break;
        }

        rectTransform.anchoredPosition = startPosition;
        canvasGroup.interactable = false;

        float time = 0f;
        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.anchoredPosition = targetPosition;
        canvasGroup.interactable = true;

        OnEnd?.Invoke();
    }

    public static IEnumerator ZoomIn(
        RectTransform rectTransform, 
        CanvasGroup canvasGroup,
        float duration, 
        Action OnEnd)
    {
        canvasGroup.interactable = false;   

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
            rectTransform.localScale = Vector2.Lerp(Vector2.zero, Vector2.one, Mathf.Clamp01(time / duration));
        }

        rectTransform.localScale = Vector2.one;
        canvasGroup.interactable = true;

        OnEnd?.Invoke();
    }

    public static IEnumerator ZoomOut(
        RectTransform rectTransform, 
        CanvasGroup canvasGroup,
        float duration, 
        Action OnEnd)
    {
        canvasGroup.interactable = false;

        float time = 0f;
        while (time < duration)
        {
            rectTransform.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.localScale = Vector2.zero;
        canvasGroup.interactable = true;

        OnEnd?.Invoke();

        yield return null;
    }

    public static IEnumerator FadeIn(
        CanvasGroup canvasGroup,
        float duration,
        Action OnEnd)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;

        float time = 0f;
        while (time < duration)
        {
            canvasGroup.alpha = time / duration;
            yield return null;
            time += Time.deltaTime;
           
        }

        canvasGroup.alpha = 1f;
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        OnEnd?.Invoke();
    }

    public static IEnumerator FadeOut(
        CanvasGroup canvasGroup,
        float duration,
        Action OnEnd)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        canvasGroup.alpha = 1f;
        float time = 0f;
        while (time < duration)
        {
            
            canvasGroup.alpha = 1 - time / duration;
            yield return null;
            time += Time.deltaTime;
            
        }

        canvasGroup.alpha = 0f;

        OnEnd?.Invoke();
    }
}