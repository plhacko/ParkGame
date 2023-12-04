using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using FreeDraw;
using Managers;
using Unity.AI;
using Unity.AI.Navigation;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.Windows;
using NavMeshSurface = NavMeshPlus.Components.NavMeshSurface;

[RequireComponent(typeof(Drawable))]
public class CreateMapWithOverlay : MonoBehaviour
{
    public Camera mainCamera; // for map scale adjustment
    // Tilemaps
    [Header("Tilemaps")]
    public Tilemap baseTilemap;
    public Tilemap actionTilemap;
    public Tilemap blockingTilemap;
    
    // Tile sprites
    [Header("Tile Sprites")]
    public TileBase pathTile;
    public TileBase boundsTile;
    public TileBase wallTile;
    public TileBase backgroundTile;

    // Structure tiles (currently unused, replaced by separate GameObject)
    [Header("Structure tiles (unused now, replaced by prefabs)")]
    public TileBase outpostTile;
    public TileBase victoryPointTile;
    public TileBase castleTile;
    // Structure prefabs (used instead of tiles)
    [Header("Structure prefabs")]
    public GameObject outpostPrefab;
    public GameObject castlePrefab;
    public GameObject victoryPointPrefab;
    // used for storing placed structure counts and positions
    [Header("Structure counters (For map drawing)")]
    public StructureCounter outposts;
    public StructureCounter victoryPoint;
    public StructureCounter castles;
    
    [Header("Additional settings")]
    public bool doNotFetch = false; // Do not try to load a map (Debug)
    public NavMeshSurface navMesh;  // used for building navMesh on tilemap
    public GridLayout gridLayout; // used for placing structures correctly based on tilemap coordinates

    public Action<bool> OnMapValidated;
    public Action<List<string>> OnMapCreationErrors;
    
    private Sprite mapOverlay;
    public Drawable mapDrawable;
    private Texture2D drawableTexture;
    private Texture2D resizedDrawableTexture;
    private Sprite drawableSprite;
    private SpriteRenderer drawableSpriteRenderer;
    private GameObject fetchedMap;
    private Vector3Int topLeftCellPos, bottomRightCellPos; // Used for tilemap recreation
    private List<string> errorMessages = new List<string>();

    [SerializeField] private GameObject mapSprite;
    public MapDisplayer BaseMap {get; private set;}

    // Start is called before the first frame update
    void Start()
    {
        drawableSpriteRenderer = GetComponent<SpriteRenderer>();
        mapDrawable = GetComponent<Drawable>();
        if (!doNotFetch) // Wait until map fetching from MapBox is completed
            StartCoroutine(WaitForValue());
    }

    public void CreateTilemapFromFetchedMap(MapData mapData)
    {
        SetLowResTextureForTilemapCreation(mapData.DrawnTexture);
        SetTilemapBounds(mapData.MetaData.TopLeftTileIdx, mapData.MetaData.BottomRightTileIdx);
        CreateTilemapFromTexture(fromUploadedTexture: true, structures: mapData.MetaData.Structures);
        navMesh.BuildNavMesh();

        var mapDisplayer = mapSprite.GetComponent<MapDisplayer>();
        mapDisplayer.mapData = mapData;
        mapDisplayer.Initiator = MapDisplayer.MapInitiator.MapData;

        fetchedMap = mapDisplayer.transform.gameObject;
        BaseMap = Instantiate(mapSprite, transform).GetComponent<MapDisplayer>();
        
        Debug.Log("Map fetched and created");
    }

    public bool IsBaseMapLoaded()
    {
        return BaseMap.IsMapLoaded();
    }

    
    
    private IEnumerator WaitForValue()
    {
        float timer = 0f;
        float maxWaitTime = 5f; // Maximum wait time in seconds
        fetchedMap = GameObject.FindWithTag("FetchedMapSprite");
        var fetchedMapRenderer = fetchedMap.GetComponent<SpriteRenderer>();
        

        // Continuously check for the value until it becomes non-null or the time limit is reached
        while (fetchedMapRenderer.sprite == null && timer < maxWaitTime)
        {
            timer += Time.deltaTime;
            yield return null; // Wait for one frame
        }

        if (fetchedMapRenderer.sprite != null)
        {
            Debug.Log("Map was fetched is now available.");
            fetchedMap.transform.SetParent(transform.parent);
            SetMapOverlay(fetchedMapRenderer.sprite);
        }
        else
        {
            Debug.LogWarning("Map was not fetched within the specified time.");
        }
    }
    
    public GameObject GetFetchedMap()
    {
        return fetchedMap;
    }
    
    public void FitCameraToMap()
    {
        // Calculate the size of the object based on its distance from the camera and its local scale
        Vector3 mapBounds = BaseMap.GetComponent<SpriteRenderer>().bounds.size;
        float objectWidth = mapBounds.x;
        float objectHeight = mapBounds.y;

        // Calculate the desired height and width of the object in the camera's view
        float frustumHeight = 2.0f * mainCamera.orthographicSize;
        float frustumWidth = frustumHeight * mainCamera.aspect;

        // Calculate the scale factor to fit the object to the camera view
        float scaleFactorHeight = frustumHeight / objectHeight;
        float scaleFactorWidth = frustumWidth / objectWidth;

        
        
        // Use the smaller scale factor to maintain aspect ratio and prevent stretching
        var scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

        // gameObject.transform.localScale *= scaleFactor;
        // fetchedMap.transform.localScale *= scaleFactor;
        mainCamera.orthographicSize /= scaleFactor;
    }   
    
    private void FitCamera()
    {
        // Calculate the size of the object based on its distance from the camera and its local scale
        Vector3 mapBounds;
        if (fetchedMap)
            mapBounds = fetchedMap.GetComponent<SpriteRenderer>().bounds.size;
        else
            mapBounds = mapOverlay.bounds.size;
        float objectWidth = mapBounds.x;
        float objectHeight = mapBounds.y;

        // Calculate the desired height and width of the object in the camera's view
        float frustumHeight = 2.0f * mainCamera.orthographicSize;
        float frustumWidth = frustumHeight * mainCamera.aspect;

        // Calculate the scale factor to fit the object to the camera view
        float scaleFactorHeight = frustumHeight / objectHeight;
        float scaleFactorWidth = frustumWidth / objectWidth;

        
        
        // Use the smaller scale factor to maintain aspect ratio and prevent stretching
        var scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

        // gameObject.transform.localScale *= scaleFactor;
        // fetchedMap.transform.localScale *= scaleFactor;
        mainCamera.orthographicSize /= scaleFactor;
    }
    private void CreateNewTextureForDrawing()
    {
        // create new texture and sprite where to draw based on resolution of map snippet
        int scaleRation = 4;
        drawableTexture = new Texture2D(mapOverlay.texture.width / scaleRation, mapOverlay.texture.height / scaleRation);
        drawableTexture.name = "DrawableTexture";
        drawableTexture.Apply();
        drawableSprite = Sprite.Create(
            drawableTexture,
            new Rect(0, 0, drawableTexture.width, drawableTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        drawableSprite.name = "DrawableSprite";
        // set the newly created sprite to the drawable script
        mapDrawable.SetDrawableSprite(drawableSprite, scaleRation);
    }
    
    /// <summary>
    /// Set map image as an overlay for drawing texture
    /// </summary>
    /// <param name="newMapOverlay"></param>
    public void SetMapOverlay(Sprite newMapOverlay)
    {
        mapOverlay = newMapOverlay;
        CreateNewTextureForDrawing();
        FitCamera();
    }

    public void SetTilemapBounds(Vector3Int newTopLeftCellPos, Vector3Int newBottomRightCellPos)
    {
        topLeftCellPos = newTopLeftCellPos;
        bottomRightCellPos = newBottomRightCellPos;
    }
    
    public Tuple<Vector3Int,Vector3Int> GetTilemapBounds()
    {
        return new Tuple<Vector3Int, Vector3Int>(topLeftCellPos, bottomRightCellPos);
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
        File.WriteAllBytes(Application.dataPath + "/Sprites/Map/customMap.png", pngData);
        Debug.Log("Map saved");
#else
        Debug.Log("Currently the map can be saved only in the editor");
#endif 
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
        // Definition of mapping from color to tiles, if adding new, also add color button to UI for drawing
        var colorToTile = new Dictionary<Color, TileBase>()
        {
            {Color.yellow, pathTile},
            {Color.blue, wallTile},
            {Color.red, boundsTile},
            {Color.clear, backgroundTile}
        };
        // calculate closest color match 
        return colorToTile[colorToTile.Keys.OrderBy(n => ColorDiff(n, target)).First()];
    }
    
    public void ClearTilemap()
    {
        baseTilemap.ClearAllTiles();
        actionTilemap.ClearAllTiles();
        blockingTilemap.ClearAllTiles();
    }
    
    public void ToggleDrawable()
    {
        if (drawableSpriteRenderer.color.a != 1)
        {
            drawableSpriteRenderer.color = Color.white;
            mapDrawable.SetDrawableState(drawingLocked: false);
        }
        else
        {
            drawableSpriteRenderer.color = new Color(1, 1, 1, 0.2f);
            mapDrawable.SetDrawableState(drawingLocked: true);
        }
    }

    private void ToggleTilemap(bool visible)
    {
        blockingTilemap.GetComponent<TilemapRenderer>().enabled = visible;
        actionTilemap.GetComponent<TilemapRenderer>().enabled = visible;
        baseTilemap.GetComponent<TilemapRenderer>().enabled = visible;
    }

    /**
     * Returns drawn texture scaled down on small resolution needed for tilemap creation 
     */
    public Texture2D GetLowResTextureForTilemapCreation()
    {
        return resizedDrawableTexture;
    }
    public void SetLowResTextureForTilemapCreation(Texture2D uploadedTexture)
    {
        resizedDrawableTexture = uploadedTexture;
        // Debug feature
        // drawableSpriteRenderer.sprite = Sprite.Create(
        //    uploadedTexture, new Rect(0, 0, uploadedTexture.width, uploadedTexture.height), Vector2.one * 0.5f
        // );
    }

    private double Dist2DSquared(Vector3Int a, Vector3Int b)
    {
        return Math.Pow(a.x-b.x,2)+Math.Pow(a.y-b.y,2);
    }
    
    private void CheckStructureDistances(
        Dictionary<Vector3Int, GameObject> structuresToAssign
    )
    {
        const int distSquaredThreshold = 5 * 5;
        for (int i = 0; i < structuresToAssign.Count; ++i)
        {
            for (int j = i + 1; j < structuresToAssign.Count; ++j)
            {
                var distSquared = Dist2DSquared(
                    structuresToAssign.ElementAt(i).Key, structuresToAssign.ElementAt(j).Key
                );
                if (distSquared < distSquaredThreshold)
                {
                    errorMessages.Add(
                        $"Distance between structures is too small " +
                        $"({structuresToAssign.ElementAt(i).Value.name} and {structuresToAssign.ElementAt(j).Value.name})"
                    );
                }
            }
        }
    }
    private void SetStructurePrefabs(Dictionary<Vector3Int, GameObject> structuresToAssign)
    {
        CheckStructureDistances(structuresToAssign);
        var castleTeamID = 0;

        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var kvp in structuresToAssign)
            {
                if (blockingTilemap.GetTile(kvp.Key) == boundsTile)
                    errorMessages.Add("Cannot place structure out of map bounds");
                var structure = Instantiate(kvp.Value, gridLayout.CellToWorld(kvp.Key), Quaternion.identity);
                if (kvp.Value == castlePrefab)
                {
                    structure.GetComponent<Outpost>().SetCastle(castleTeamID);
                    castleTeamID++;
                }
                var networkObject = structure.GetComponent<NetworkObject>();
                networkObject.Spawn();
            }
        }
    }
    private void SetStructureTiles(Dictionary<Vector3Int, TileBase> structuresToAssign,List<string> errorMessages)
    {
        var structureRadius = 2;
        foreach (var kvp in structuresToAssign)
        {
            for (int x = -structureRadius; x <= structureRadius; x++)
            {
                for (int y = -structureRadius; y <= structureRadius; y++)
                {
                    var offsetPosition = kvp.Key + new Vector3Int(x, y, 0);
                    if (actionTilemap.GetTile(offsetPosition) != boundsTile)
                        actionTilemap.SetTile(offsetPosition, kvp.Value);
                    else
                        throw new ArgumentException("Cannot place structure out of map bounds");
                }
            }
        }
    }

    /**
     * Helper function for creating tilemap without parameters so it can be set in button OnClick section
     */
    public void ProcessTilemap()
    {
        CreateTilemapFromTexture(false, null);
        if (errorMessages.Count != 0)
        {
            // foreach (var errorMessage in errorMessages)
            // {
            //     Debug.LogError(errorMessage);
            // }
            ToggleTilemap(false);  // Hide tilemap
            OnMapCreationErrors.Invoke(errorMessages);
            return;
        }
        ToggleTilemap(true);
        OnMapValidated.Invoke(true); // Enable SaveMap button
    }
    /**
     * Creates tilemap from drawn texture by scaling texture to low resolution and assigning tiles by according colors
     */
    private void CreateTilemapFromTexture(bool fromUploadedTexture = false, MapStructures structures = null)
    {
        errorMessages = new List<string>();
        if (!fromUploadedTexture)
            OnMapValidated.Invoke(false); // Disable Save map button, reenabled after valid map creation
        ClearTilemap();

        if (!fromUploadedTexture)
        {
            var spriteRectVertices = drawableSprite.vertices;
            var worldMat = transform.localToWorldMatrix;
            topLeftCellPos = baseTilemap.WorldToCell(worldMat * spriteRectVertices[0]);
            bottomRightCellPos = baseTilemap.WorldToCell(worldMat * spriteRectVertices[1]);
        }

        if (topLeftCellPos == bottomRightCellPos)
        {
            errorMessages.Add("Draw your map first");
            return;
        }
        // Put tile to the opposite corners to set correct tilemap bounds so fill works correctly,
        // tilemap API doesnt do this automatically...
        blockingTilemap.SetTile(topLeftCellPos + new Vector3Int(-2, 2, 0), boundsTile);
        blockingTilemap.SetTile(bottomRightCellPos + new Vector3Int(2, -2, 0), boundsTile);
        
        var widthTilemap = bottomRightCellPos.x - topLeftCellPos.x;
        var heightTilemap = topLeftCellPos.y - bottomRightCellPos.y;
        // CreateOutsideBoundary();
        
        if (!fromUploadedTexture)
        {
            resizedDrawableTexture = TextureScaler.scaled(drawableTexture, widthTilemap, heightTilemap, FilterMode.Point);
            // SaveMap(resizedDrawableTexture);
        }
        var texturePixels = resizedDrawableTexture.GetPixels();

        var tilesToAssign = new Dictionary<Vector3Int, TileBase>();
        for (var j = bottomRightCellPos.y; j < topLeftCellPos.y; j++)
        {
            for (var i = topLeftCellPos.x; i < bottomRightCellPos.x; i++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                // var tile = tilemap.GetTile(tilePos);
                int pixelIdx = (j - bottomRightCellPos.y) * widthTilemap + (i - topLeftCellPos.x);
                var pixelColor = texturePixels[pixelIdx];
                var colorMatchingTile = ClosestColor(pixelColor);
                if (colorMatchingTile == boundsTile || colorMatchingTile == wallTile)
                    blockingTilemap.SetTile(tilePos, colorMatchingTile);
                else
                    baseTilemap.SetTile(tilePos, colorMatchingTile);
                    // tilesToAssign.Add(tilePos, colorMatchingTile);
            }
            
        }
        // Flood fill the boundary before setting rest of the tiles
        blockingTilemap.FloodFill(
            new Vector3Int(topLeftCellPos.x, topLeftCellPos.y), boundsTile
            );
        // TODO check that whole map is not red
        if (blockingTilemap.GetTilesBlock(blockingTilemap.cellBounds).All(tile => tile == boundsTile))
        {
            errorMessages.Add("Bounds (Red color) must be enclosed area");
            return;
        }
        var structuresToAssign = new Dictionary<Vector3Int, GameObject>();
        try
        {
            if (structures == null)
            {
                if (castles.GetStructureCount() < 2)
                {
                    errorMessages.Add("Place at least 2 castles to the map.");
                    return;
                }

                if (victoryPoint.GetStructureCount() == 0)
                {
                    errorMessages.Add("Add victory point to the map");
                    return;
                }
                // Also for adjusting uploaded tilemap (Not used for now)
                foreach (var tilePos in outposts.GetPlacedStructurePositions().Item2)
                    structuresToAssign.Add(tilePos, outpostPrefab);
                foreach (var tilePos in castles.GetPlacedStructurePositions().Item2)
                    structuresToAssign.Add(tilePos, castlePrefab);
                foreach (var tilePos in victoryPoint.GetPlacedStructurePositions().Item2)
                    structuresToAssign.Add(tilePos, victoryPointPrefab);
            }
            else
            {
                // Structures from uploaded map 
                foreach (var tilePos in structures.Outposts)
                    structuresToAssign.Add(new Vector3Int(tilePos.x, tilePos.y, tilePos.z), outpostPrefab);
                foreach (var tilePos in structures.Castles)
                    structuresToAssign.Add(new Vector3Int(tilePos.x, tilePos.y, tilePos.z), castlePrefab);
                foreach (var tilePos in structures.VictoryPoints)
                    structuresToAssign.Add(new Vector3Int(tilePos.x, tilePos.y, tilePos.z), victoryPointPrefab);
            }
        }
        catch(ArgumentException)
        {
            errorMessages.Add("Structures are overlapping");
            return;
        }

        // foreach (var kvp in tilesToAssign)
        // {
        //     if (tilemap.GetTile(kvp.Key) == null)
        //         tilemap.SetTile(kvp.Key, kvp.Value);
        // }
        SetStructurePrefabs(structuresToAssign);
        // SetStructureTiles(structuresToAssign);
    }
}
