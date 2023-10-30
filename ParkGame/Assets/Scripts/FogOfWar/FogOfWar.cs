using UnityEngine;
using UnityEngine.Serialization;

public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer fog;
    [SerializeField] private SpriteRenderer revealer;
    [FormerlySerializedAs("HiddenColor")] [SerializeField] private Color32 hiddenColor = new Color32(0, 0, 0, 200);
    [SerializeField] private int fogSizeInUnits = 100;
    [SerializeField] private int pixelsPerUnit = 32;
    [SerializeField] private int revealRadius = 8;
    [SerializeField] private int edgeWidth = 3;
    
    private int textureSize;
    private Texture2D fogTexture;
    private Color32[] fogBuffer;
    
    private Texture2D revealerTexture;
    private Color32[] revealerBuffer;
   
    void Awake()
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
    }

    private void OnDestroy()
    {
        fogTexture.Destroy();
        revealerTexture.Destroy();
    }

    void Update()
    {
        Vector2 position = transform.position;
        position -= new Vector2(0.5f * textureSize / pixelsPerUnit, 0.5f * textureSize / pixelsPerUnit);
        
        Vector2 targetPosition = target.position;
        float squareRevealRadius = (revealRadius / (float)pixelsPerUnit) * (revealRadius / (float)pixelsPerUnit);
        float squarePartialRevealRadius = ((revealRadius + edgeWidth) / (float)pixelsPerUnit) * ((revealRadius + edgeWidth) / (float)pixelsPerUnit);
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 pixelWorldPosition = position + new Vector2(x / (float)pixelsPerUnit, y / (float)pixelsPerUnit);
                
                Color32 revealerColor = new Color32(0, 0, 0, 255);
                Color32 fogColor;
                float sqrDistance = Vector2.SqrMagnitude(pixelWorldPosition - targetPosition);
                if (sqrDistance < squareRevealRadius)
                {
                    fogColor = new Color32(0, 0, 0, 0);
                }
                else if (sqrDistance < squarePartialRevealRadius)
                {
                    float distance = Mathf.Sqrt(sqrDistance);
                    float alpha = (distance - revealRadius / (float)pixelsPerUnit) / edgeWidth;
                    fogColor = Color32.Lerp(new Color32(0, 0, 0, 0), hiddenColor, alpha);
                }
                else
                {
                    fogColor = hiddenColor;
                    revealerColor = new Color32(0, 0, 0, 0);
                }
                
                fogBuffer[x + y * textureSize] = fogColor;
                revealerBuffer[x + y * textureSize] = revealerColor;
            }
        }
        fogTexture.SetPixels32(fogBuffer);
        fogTexture.Apply();
        
        revealerTexture.SetPixels32(revealerBuffer);
        revealerTexture.Apply();
    }
}
