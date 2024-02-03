using System.Collections.Generic;
using UnityEngine;

public class UIOutpostListController : MonoBehaviour
{
    [SerializeField] private UIOutpost outpostPrefab;
    private Dictionary<Outpost, UIOutpost> outposts = new Dictionary<Outpost, UIOutpost>();

    public void AddOutpost(Outpost outpost)
    {
        var outpostUI = Instantiate(outpostPrefab, transform);
        outposts.Add(outpost, outpostUI);
        outpostUI.Initialize(outpost, () => RemoveOutpost(outpost));
    }

    public void RemoveOutpost(Outpost outpost)
    {
        if (outposts.ContainsKey(outpost))
        {
            Destroy(outposts[outpost].gameObject);
            outposts.Remove(outpost);
        }
    }
}
