using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Shader Shader;

    public void Change()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = new Material(Shader);
        }
    }
}
