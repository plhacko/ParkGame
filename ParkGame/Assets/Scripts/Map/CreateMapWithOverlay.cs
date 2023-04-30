using System;
using System.Collections;
using System.Collections.Generic;
using FreeDraw;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Windows;

[RequireComponent(typeof(Drawable))]
public class CreateMapWithOverlay : MonoBehaviour
{
    [SerializeField] Sprite mapOverlay;

    private Texture2D drawableTexture;
    private Sprite drawableSprite;
    private String path = Application.dataPath + "/Sprites/Map/customMap.png";
    
    // Start is called before the first frame update
    void Start()
    {
        if (mapOverlay)
            CreateNewTextureForDrawing();
    }

    private void CreateNewTextureForDrawing()
    {
        // create new texture and sprite where to draw based on resolution of map snippet
        drawableTexture = new Texture2D(mapOverlay.texture.width, mapOverlay.texture.height);
        drawableTexture.name = "DrawableTexture";
        drawableTexture.Apply();
        drawableSprite = Sprite.Create(
            drawableTexture,
            new Rect(0, 0, drawableTexture.width, drawableTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        drawableSprite.name = "DrawableSprite";
        // set the newly created sprite to the drawable script
        GetComponent<Drawable>().SetDrawableSprite(drawableSprite);
    }
    
    /// <summary>
    /// Set map image as an overlay for drawing texture
    /// </summary>
    /// <param name="newMapOverlay"></param>
    public void SetMapOverlay(Sprite newMapOverlay)
    {
        mapOverlay = newMapOverlay;
        CreateNewTextureForDrawing();
    }

    /// <summary>
    /// Save drawn map into file into `path` set in class variable
    /// </summary>
    public void SaveMap()
    {
        var pngData = drawableTexture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.Log("Could not convert texture to png!");
            return;
        }
        File.WriteAllBytes(path, pngData);
        Debug.Log("Map saved");
    }
}
