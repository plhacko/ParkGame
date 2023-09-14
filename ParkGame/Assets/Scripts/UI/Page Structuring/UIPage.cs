using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public bool ExitOnNextPage = false;
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
                break;
            case PageEntryMode.Fade:
                FadeIn();
                break;
            case PageEntryMode.SlideRight:
                SlideIn(Direction.Right);
                break;
            case PageEntryMode.SlideLeft:
                SlideIn(Direction.Left);
                break;
            case PageEntryMode.SlideUp:
                SlideIn(Direction.Up);
                break;
            case PageEntryMode.SlideDown:
                SlideIn(Direction.Down);
                break;
            case PageEntryMode.Zoom:
                ZoomIn();
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
                break;
            case PageEntryMode.Fade:
                FadeOut();
                break;
            case PageEntryMode.SlideRight:
                SlideOut(Direction.Right);
                break;
            case PageEntryMode.SlideLeft:
                SlideOut(Direction.Left);
                break;
            case PageEntryMode.SlideUp:
                SlideOut(Direction.Up);
                break;
            case PageEntryMode.SlideDown:
                SlideOut(Direction.Down);
                break;
            case PageEntryMode.Zoom:
                ZoomOut();
                break;
        }

        if (playSound)
        {
            PlayExitClip();
        }
    }

    private void ZoomIn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomIn(rectTransform, animationDuration, null));
    }

    private void SlideIn(Direction direction)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        switch (entryMode)
        {
            case PageEntryMode.SlideRight:
                animationCoroutine = StartCoroutine(PageAnimator.SlideIn(rectTransform, Direction.Right, animationDuration, null));
                break;
        }
        animationCoroutine = StartCoroutine(PageAnimator.SlideOut(rectTransform, direction, animationDuration, null));
    }

    private void FadeIn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeIn(canvasGroup, animationDuration, null));
    }

    private void ZoomOut()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.ZoomOut(rectTransform, animationDuration, null));
    }

    private void SlideOut(Direction direction)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.SlideOut(rectTransform, direction, animationDuration, null));
    }

    private void FadeOut()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PageAnimator.FadeOut(canvasGroup, animationDuration, null));
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
