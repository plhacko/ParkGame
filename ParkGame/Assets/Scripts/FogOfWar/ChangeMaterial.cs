using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Serialization;

public class ChangeMaterial : MonoBehaviour
{
    [FormerlySerializedAs("Shader")] [SerializeField] private Shader shader1;
    [SerializeField] private Shader shader2;

    [SerializeField] private List<GameObject> excludeObjects;

    public void Change(bool setFirst)
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var spriteRenderer = spriteRenderers[i];
            if (!excludeObjects.Contains(spriteRenderer.gameObject))
            {
                if (setFirst)
                {
                    spriteRenderer.material = new Material(shader1);
                }
                else
                {
                    spriteRenderer.material = new Material(shader2);
                }

                // team color has been lost and we need to Initialize it again
                if (spriteRenderer.TryGetComponent<ISoldier>(out ISoldier soldier))
                {
                    soldier.InitializeTeamColor();
                }
            }
        }
    }
}
