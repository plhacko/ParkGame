using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fog;
    [SerializeField] private SpriteRenderer revealer;
    [SerializeField] private Color32 hiddenColor = new Color32(0, 0, 0, 200);
    [SerializeField] private int fogSizeInUnits = 100;
    [SerializeField] private int pixelsPerUnit = 32;
    [SerializeField] private int edgeWidth = 3;
    [SerializeField] private bool debug = false;

    private int textureSize;
    private Texture2D fogTexture;
    private Color32[] fogBuffer;
    
    private Texture2D revealerTexture;
    private Color32[] revealerBuffer;
    [SerializeField] private List<Revealer> revealerTargets = new List<Revealer>();
    
    private void Awake()
    {
        textureSize = fogSizeInUnits * pixelsPerUnit;
        fogBuffer = new Color32[textureSize * textureSize];

        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        fogTexture.SetPixels32(fogBuffer);
        fogTexture.filterMode = FilterMode.Point;
        fogTexture.Apply();
        fog.sprite = Sprite.Create(fogTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        
        revealerBuffer = new Color32[textureSize * textureSize];
        revealerTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        revealerTexture.SetPixels32(fogBuffer);
        revealerTexture.filterMode = FilterMode.Point;
        revealerTexture.Apply();
        revealer.sprite = Sprite.Create(revealerTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        
        Debug.Log("fog of war initialized");
    }

    private void OnDestroy()
    {
        fogTexture.Destroy();
        revealerTexture.Destroy();
    }

    public void RegisterAsRevealer(Revealer target)
    {
        Debug.Log(target.gameObject.name + " registered as revealer");
        revealerTargets.Add(target);
    }
    
    public void DeregisterAsRevealer(Revealer target)
    {
        revealerTargets.Remove(target);
    }
    
    private void Update()
    {
        Vector2 position = transform.position;
        position -= new Vector2(0.5f * textureSize / pixelsPerUnit, 0.5f * textureSize / pixelsPerUnit);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 pixelWorldPosition = position + new Vector2(x / (float)pixelsPerUnit, y / (float)pixelsPerUnit);
                
                float maxVisibility = 0;
                for (int i = 0; i < revealerTargets.Count; i++)
                {
                    var revealerTarget = revealerTargets[i];
        
                    float squareRevealRadius = (revealerTarget.radius / (float)pixelsPerUnit) * (revealerTarget.radius / (float)pixelsPerUnit);
                    float squarePartialRevealRadius = ((revealerTarget.radius + edgeWidth) / (float)pixelsPerUnit) * ((revealerTarget.radius + edgeWidth) / (float)pixelsPerUnit);
                    
                    float sqrDistance = Vector2.SqrMagnitude(pixelWorldPosition - (Vector2)revealerTarget.transform.position);
                    if (sqrDistance < squareRevealRadius)
                    {
                        maxVisibility = 1;
                        break;
                    }
                    
                    if (sqrDistance < squarePartialRevealRadius)
                    {
                        float distance = Mathf.Sqrt(sqrDistance);
                        float visibility = 1 - (distance - revealerTarget.radius / (float)pixelsPerUnit) / edgeWidth;
                        maxVisibility = Mathf.Max(maxVisibility, visibility);
                    }
                }

                fogBuffer[x + y * textureSize] = Color32.Lerp(hiddenColor, new Color32(0, 0, 0, 0), maxVisibility);
                revealerBuffer[x + y * textureSize] = maxVisibility > 0 ? new Color32(0, 0, 0, 255) : new Color32(0, 0, 0, 0);

                if (debug)
                {
                    fogBuffer[x + y * textureSize] = new Color32(0, 0, 0, 0);
                    revealerBuffer[x + y * textureSize] = new Color32(0, 0, 0, 255);
                }
            }
        }
        fogTexture.SetPixels32(fogBuffer);
        fogTexture.Apply();
        
        revealerTexture.SetPixels32(revealerBuffer);
        revealerTexture.Apply();
    }
}
