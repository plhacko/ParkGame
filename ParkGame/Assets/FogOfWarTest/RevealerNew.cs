using UnityEngine;

public class RevealerNew : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int pixelsPerUnit = 1;
    [SerializeField] private int radius;
    
    private static readonly int PixelsPerUnit = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int Radius = Shader.PropertyToID("_Radius");
    private static readonly int Position = Shader.PropertyToID("_Position");

    private void Awake()
    {
        spriteRenderer.material.SetInt(PixelsPerUnit, pixelsPerUnit);
        spriteRenderer.material.SetInt(Radius, radius);
        spriteRenderer.material.SetInt(PixelsPerUnit, pixelsPerUnit);
        
        transform.localScale = new Vector3(radius * 2 / transform.parent.localScale.x, radius * 2 / transform.parent.localScale.y, 1);
    }
    
    private void Update()
    {
        spriteRenderer.material.SetVector(Position, transform.position);
    }
}
