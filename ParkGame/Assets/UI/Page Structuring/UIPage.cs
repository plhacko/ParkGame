using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(AudioSource), typeof(CanvasGroup), typeof(UIPageController))]
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
    private PageEntryMode entryMode = PageEntryMode.None;
    [SerializeField]
    private PageEntryMode exitMode = PageEntryMode.None;

    private Coroutine animationCoroutine;
    private Coroutine audioCoroutine;

    private UIPageController pageController;
    [SerializeField] private Action onEnter;
    [SerializeField] private Action onExit;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.enabled = false;

        pageController = GetComponent<UIPageController>();
        onEnter = pageController.OnEnter;
        onExit = pageController.OnExit;

        PageAnimator.PrepareAnimation(rectTransform, canvasGroup, entryMode);
    }

    public void Enter(bool playSound)
    {
        switch (entryMode)
        {
            case PageEntryMode.None:
                NoneIn(onEnter);
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
                NoneOut(onExit);
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

        PageAnimator.PrepareAnimation(rectTransform, canvasGroup, entryMode);
    }

    private void NoneIn(Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeIn(canvasGroup, 0f, callback));
    }

    private void ZoomIn(Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomIn(rectTransform, canvasGroup, animationDuration, callback));
    }

    private void SlideIn(Direction direction, Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.SlideIn(rectTransform, canvasGroup, direction, animationDuration, callback));
    }

    private void FadeIn(Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeIn(canvasGroup, animationDuration, callback));
    }

    private void NoneOut(Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeOut(canvasGroup, 0f, callback));
    }

    private void ZoomOut(Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomOut(rectTransform, canvasGroup, animationDuration, callback));
    }

    private void SlideOut(Direction direction, Action callback)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.SlideOut(rectTransform, canvasGroup, direction, animationDuration, callback));
    }

    private void FadeOut(Action callback)
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
