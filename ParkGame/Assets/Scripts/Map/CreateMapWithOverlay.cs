using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FreeDraw;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.Windows;

[RequireComponent(typeof(Drawable))]
public class CreateMapWithOverlay : MonoBehaviour
{
    [SerializeField] Sprite mapOverlay;
    public Tilemap tilemap;
    public TileBase pathTile;
    public TileBase boundsTile;
    public TileBase wallTile;
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
    public void SaveMap(Texture2D texture = null)
    {
        if (texture == null)
            texture = drawableTexture;
#if UNITY_EDITOR
        var pngData = texture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.Log("Could not convert texture to png!");
            return;
        }
        File.WriteAllBytes(path, pngData);
        Debug.Log("Map saved");
#else
        Debug.Log("Currently the map can be saved only in the editor");
#endif 
    }

    private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
        // https://answers.unity.com/questions/150942/texture-scale.html
        Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,false);
        float incX=(1.0f / (float)targetWidth);
        float incY=(1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i) {
            for (int j = 0; j < result.width; ++j) {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }

    private float ColorDiff(Color color1, Color color2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(color2.r - color1.r, 2) +
            Mathf.Pow(color2.g - color1.g, 2) +
            Mathf.Pow(color2.b - color1.b, 2) +
            Mathf.Pow(color2.a - color1.a, 2)
        );
    }
    private TileBase ClosestColor(Color target)
    {
        var colorToTile = new Dictionary<Color, TileBase>()
        {
            {Color.yellow, pathTile},
            {Color.blue, wallTile},
            {Color.red, boundsTile},
            {Color.clear, null}
        };
        return colorToTile[colorToTile.Keys.OrderBy(n => ColorDiff(n, target)).First()];
    }
    
    public void ClearTilemap()
    {
        tilemap.ClearAllTiles();
    }
    public void CreateTilemapFromTexture()
    {
        ClearTilemap();
        
        var spriteRectVertices = drawableSprite.vertices;
        var topLeftCellPos = tilemap.WorldToCell(spriteRectVertices[0]);
        var bottomRightCellPos = tilemap.WorldToCell(spriteRectVertices[1]);
        // Put tile to the opposite corners to set correct tilemap bounds so fill works correctly,
        // tilemap API doesnt do this automatically...
        tilemap.SetTile(topLeftCellPos + new Vector3Int(-2, 2, 0), boundsTile);
        tilemap.SetTile(bottomRightCellPos + new Vector3Int(2, -2, 0), boundsTile);
        
        var widthTilemap = bottomRightCellPos.x - topLeftCellPos.x;
        var heightTilemap = topLeftCellPos.y - bottomRightCellPos.y;
        // CreateOutsideBoundary();
        
        
        var resizedTexture = TextureScaler.scaled(drawableTexture, widthTilemap, heightTilemap, FilterMode.Point);
        var texturePixels = resizedTexture.GetPixels();
        SaveMap(resizedTexture);

        
        
        for (var j = bottomRightCellPos.y; j < topLeftCellPos.y; j++)
        {
            for (var i = topLeftCellPos.x; i < bottomRightCellPos.x; i++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                // var tile = tilemap.GetTile(tilePos);
                int pixelIdx = (j - bottomRightCellPos.y) * widthTilemap + (i - topLeftCellPos.x);
                var pixelColor = texturePixels[pixelIdx];
                tilemap.SetTile(tilePos, ClosestColor(pixelColor));
            }
            
        }

        tilemap.FloodFill(
            new Vector3Int(topLeftCellPos.x, topLeftCellPos.y), boundsTile
            );
    }
}
