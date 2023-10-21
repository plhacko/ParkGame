using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(AudioSource), typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class UIPage : MonoBehaviour
{
    private AudioSource audioSource;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float animationDuration = 1f;
    public bool ExitOnNextPage = true;
    [SerializeField]
    private AudioClip EntryClip;
    [SerializeField]
    private AudioClip ExitClip;

    [SerializeField]
    private PageEntryMode entryMode = PageEntryMode.SlideRight;
    [SerializeField]
    private PageEntryMode exitMode = PageEntryMode.SlideLeft;

    private Coroutine animationCoroutine;
    private Coroutine audioCoroutine;

    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.enabled = false;
    }

    public void Enter(bool playSound)
    {
        switch (entryMode)
        {
            case PageEntryMode.None:
                None(onEnter);
                break;
            case PageEntryMode.Fade:
                FadeIn(onEnter);
                break;
            case PageEntryMode.SlideRight:
                SlideIn(Direction.Right, onEnter);
                break;
            case PageEntryMode.SlideLeft:
                SlideIn(Direction.Left, onEnter);
                break;
            case PageEntryMode.SlideUp:
                SlideIn(Direction.Up, onEnter);
                break;
            case PageEntryMode.SlideDown:
                SlideIn(Direction.Down, onEnter);
                break;
            case PageEntryMode.Zoom:
                ZoomIn(onEnter);
                break;
        }

        if (playSound)
        {
            PlayEntryClip();
        }
    }

    public void Exit(bool playSound) 
    {
        switch (exitMode)
        {
            case PageEntryMode.None:
                None(onExit);
                break;
            case PageEntryMode.Fade:
                FadeOut(onExit);
                break;
            case PageEntryMode.SlideRight:
                SlideOut(Direction.Right, onExit);
                break;
            case PageEntryMode.SlideLeft:
                SlideOut(Direction.Left, onExit);
                break;
            case PageEntryMode.SlideUp:
                SlideOut(Direction.Up, onExit);
                break;
            case PageEntryMode.SlideDown:
                SlideOut(Direction.Down, onExit);
                break;
            case PageEntryMode.Zoom:
                ZoomOut(onExit);
                break;
        }

        if (playSound)
        {
            PlayExitClip();
        }
    }

    private void None(UnityEvent callback)
    {
        callback?.Invoke();
    }

    private void ZoomIn(UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomIn(rectTransform, animationDuration, callback));
    }

    private void SlideIn(Direction direction, UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.SlideIn(rectTransform, direction, animationDuration, callback));
    }

    private void FadeIn(UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeIn(canvasGroup, animationDuration, callback));
    }

    private void ZoomOut(UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomOut(rectTransform, animationDuration, callback));
    }

    private void SlideOut(Direction direction, UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.SlideOut(rectTransform, direction, animationDuration, callback));
    }

    private void FadeOut(UnityEvent callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeOut(canvasGroup, animationDuration, callback));
    }

    public void PlayEntryClip()
    {
        if (audioSource == null || EntryClip == null)
        {
            return;
        }

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }

        audioCoroutine = StartCoroutine(PlayClip(EntryClip));
    }

    public void PlayExitClip()
    {
        if (audioSource == null || ExitClip == null)
        {
            return;
        }

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }

        audioCoroutine = StartCoroutine(PlayClip(ExitClip));
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        audioSource.enabled = true;

        audioSource.PlayOneShot(clip);

        yield return new WaitForSeconds(clip.length);

        audioSource.enabled = false;
    }
}
