using System.Collections.Generic;
using UnityEngine;

public class UIOutpostListController : MonoBehaviour
{
    [SerializeField] private UIOutpost outpostPrefab;
    [SerializeField] private UIOutpost castlePrefab;
    private Dictionary<Outpost, UIOutpost> outposts = new Dictionary<Outpost, UIOutpost>();

    public void AddOutpost(Outpost outpost)
    {
        // var uiPrefab = outpost.IsCastle ? outpostPrefab : castlePrefab; // todo create prefab for castle ui and use it here
        var uiPrefab = castlePrefab;
        var outpostUI = Instantiate(uiPrefab, transform);

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
