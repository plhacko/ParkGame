using Unity.Netcode;

public class Castle : NetworkBehaviour {
    public int Team { get => outpost.Team; set => outpost.Team = value; }

    private Outpost outpost;
    
    private void Start() {
        outpost = GetComponent<Outpost>();
    }
}
