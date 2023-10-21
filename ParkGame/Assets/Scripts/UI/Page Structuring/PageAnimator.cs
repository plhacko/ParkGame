using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PageAnimator
{
    public static IEnumerator SlideIn(
        RectTransform rectTransform,
        Direction direction, 
        float duration, 
        UnityEvent OnEnd)
    {
        Vector2 targetPosition = Vector2.zero;
        Vector2 startPosition = Vector2.zero;

        switch (direction)
        {
            case Direction.Left:
                startPosition.x = Screen.width;
                break;
            case Direction.Right:
                startPosition.x -= Screen.width;
                break;
            case Direction.Up:
                startPosition.y -= Screen.height;
                break;
            case Direction.Down:
                startPosition.y += Screen.height;
                break;
        }

        rectTransform.anchoredPosition = startPosition;

        float time = 0f;
        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.anchoredPosition = targetPosition;

        OnEnd?.Invoke();
    }

    public static IEnumerator SlideOut(
        RectTransform rectTransform,
        Direction direction,
        float duration,
        UnityEvent OnEnd)
    {
        Vector2 targetPosition = Vector2.zero;
        Vector2 startPosition = Vector2.zero;

        switch (direction)
        {
            case Direction.Left:
                targetPosition.x -= Screen.width;
                break;
            case Direction.Right:
                targetPosition.x += Screen.width;
                break;
            case Direction.Up:
                targetPosition.y += Screen.height;
                break;
            case Direction.Down:
                targetPosition.y -= Screen.height;
                break;
        }

        rectTransform.anchoredPosition = startPosition;

        float time = 0f;
        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.anchoredPosition = targetPosition;

        OnEnd?.Invoke();
    }

    public static IEnumerator ZoomIn(
        RectTransform rectTransform, 
        float duration, 
        UnityEvent OnEnd)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
            rectTransform.localScale = Vector2.Lerp(Vector2.zero, Vector2.one, Mathf.Clamp01(time / duration));
        }

        rectTransform.localScale = Vector2.one;

        OnEnd?.Invoke();
    }

    public static IEnumerator ZoomOut(
        RectTransform rectTransform, 
        float duration, 
        UnityEvent OnEnd)
    {
        float time = 0f;
        while (time < duration)
        {
            rectTransform.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, Mathf.Clamp01(time / duration));
            yield return null;
            time += Time.deltaTime;
        }

        rectTransform.localScale = Vector2.zero;

        OnEnd?.Invoke();

        yield return null;
    }

    public static IEnumerator FadeIn(
        CanvasGroup canvasGroup,
        float duration,
        UnityEvent OnEnd)
    {
        canvasGroup.alpha = 0f;
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
        UnityEvent OnEnd)
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