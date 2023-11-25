using UnityEngine;

public class FogOfWarNew : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fogOfWarSpriteRenderer;
    [SerializeField] private SpriteRenderer revealerSpriteRenderer;
    [SerializeField] private int pixelsPerUnit = 1;
    [SerializeField] private int width;
    [SerializeField] private int radius;
    [SerializeField] private int radiusEdge;
    [SerializeField] private Transform target;
    [SerializeField] private Color hiddenColor;
    
    private static readonly int RevealersPositions = Shader.PropertyToID("_RevealersPositions");
    private static readonly int RevealersCount = Shader.PropertyToID("_RevealersCount");
    private static readonly int PixelsPerUnit = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int RevealersRadii = Shader.PropertyToID("_RevealersRadii");
    private static readonly int RadiusEdge = Shader.PropertyToID("_RadiusEdge");
    private static readonly int HiddenColor = Shader.PropertyToID("_HiddenColor");

    void Start()
    {
        fogOfWarSpriteRenderer.transform.localScale = new Vector3(width, width, 1);
        fogOfWarSpriteRenderer.material.SetInt(Width, width);
        
        revealerSpriteRenderer.transform.localScale = new Vector3(width, width, 1);
        revealerSpriteRenderer.material.SetInt(Width, width);
    }

    // Update is called once per frame
    void Update()
    {
        var radii = new float[] { radius };
        var positions = new[] { new Vector4(target.position.x, target.position.y) };
        
        revealerSpriteRenderer.material.SetFloatArray(RevealersRadii, radii);
        revealerSpriteRenderer.material.SetVectorArray(RevealersPositions, positions);
        revealerSpriteRenderer.material.SetInt(RevealersCount, positions.Length);
        revealerSpriteRenderer.material.SetInt(PixelsPerUnit, pixelsPerUnit);
        revealerSpriteRenderer.material.SetInt(RadiusEdge, radiusEdge);
        
        fogOfWarSpriteRenderer.material.SetFloatArray(RevealersRadii, radii);
        fogOfWarSpriteRenderer.material.SetVectorArray(RevealersPositions, positions);
        fogOfWarSpriteRenderer.material.SetInt(RevealersCount, positions.Length);
        fogOfWarSpriteRenderer.material.SetInt(PixelsPerUnit, pixelsPerUnit);
        fogOfWarSpriteRenderer.material.SetInt(RadiusEdge, radiusEdge);
        fogOfWarSpriteRenderer.material.SetColor(HiddenColor, hiddenColor);
    }
}
