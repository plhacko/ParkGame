using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasMapFitter : MonoBehaviour
{
    public void Fit(Bounds bounds)
    {

        var canvas = GetComponent<Canvas>();
        var canvasRect = canvas.GetComponent<RectTransform>();

        var scale = CalculateFitMapScale(bounds);

        canvasRect.localScale = new Vector3(scale, scale, 1);
    }

    public float CalculateFitMapScale(Bounds bounds)
    {
        var canvas = GetComponent<Canvas>();
        canvas.transform.position = Vector3.zero;
        // uniform scale the canvas to fit the bounds
        var canvasRect = canvas.GetComponent<RectTransform>();
        var canvasSize = canvasRect.sizeDelta;
        var boundsSize = bounds.size;

        var xScale = boundsSize.x / canvasSize.x;
        var yScale = boundsSize.y / canvasSize.y;

        var scale = Mathf.Min(xScale, yScale);

        return scale;
    }
}
