using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitViewToGameUI : MonoBehaviour
{
    [SerializeField] private RectTransform gameView;

    void Awake()
    {
        var gameViewHeight = gameView.anchorMin.y;
        Camera.main.rect = new Rect(0, gameViewHeight, 1, 1 - gameViewHeight);
    }
}
