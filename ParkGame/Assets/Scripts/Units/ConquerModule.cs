using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConquerModule : NetworkBehaviour
{
    // public int Team // used as a wrapper around the parent object Team
    // {
    //     get => TeamMember != null ? TeamMember.Team : -1;
    //     set { if (TeamMember != null) { TeamMember.Team = value; } }
    // }
    [SerializeField] IConquerable conquerable = null;
    [SerializeField] NetworkVariable<float> _ConquerPoints = new();
    [SerializeField] float ConquerPointsRequired = 50.0f;
    [SerializeField] List<ITeamMember> VisibleConquerUnits = new();
    [SerializeField] List<ITeamMember> VisibleOtherUnits = new();
    
    public VictoryPoint victoryPoint;

    public float ConquerPoints { get => _ConquerPoints.Value; set => _ConquerPoints.Value = value; }

    private IProgressBar ProgressBar = null;
    private int ConquererTeam => VisibleConquerUnits.Count > 0 ? VisibleConquerUnits[0].Team : -1;

    // public UnityAction<float> OnConquerPointsChanged;

    private void Start()
    {
        // Require component in this gameobject or its parent
        if (gameObject.GetComponentInParent<Rigidbody2D>() == null && gameObject.GetComponent<Rigidbody2D>() == null) {
            Debug.LogError("Rigidbody2D is missing from the object and its parent.");
        }

        // initialize the team member (the object we will be setting Team)
        conquerable = transform.parent.GetComponent<IConquerable>();
        // progress bar
        ProgressBar = GetComponentInChildren<IProgressBar>();
        if (ProgressBar != null)
        {
            ProgressBar?.SetMaxValue(ConquerPointsRequired);
            _ConquerPoints.OnValueChanged += UpdateProgressBarDelegate;
        }
        
        // if on victory point
        victoryPoint = gameObject.GetComponentInParent<VictoryPoint>();

    }
    void UpdateProgressBarDelegate(float oldValue, float newValue) => ProgressBar?.SetValue(newValue);

    private void Update()
    {
        // update should happen only on server
        if (!NetworkManager.Singleton.IsServer)
        { return; }

        // stole scoring points if there are teams visible 
        if (VisibleConquerUnits.Count > 0 && VisibleOtherUnits.Count > 0)
        { return; }

        if (ConquererTeam == -1)
        {
            if(VisibleConquerUnits.Count > 0)
            {
                if (conquerable.GetTeam() != ConquererTeam)
                {
                    conquerable.OnStartedConquering(ConquererTeam);   
                }
            }
            else if (VisibleOtherUnits.Count > 0)
            {
                if (conquerable.GetTeam() != ConquererTeam)
                {
                    conquerable.OnStartedConquering(ConquererTeam);
                }
                
                VisibleConquerUnits.AddRange(VisibleOtherUnits);
                VisibleOtherUnits.Clear();
            }
        }
        
        if (VisibleConquerUnits.Count > 0)
        {
            if (conquerable.GetTeam() != VisibleConquerUnits[0].Team)
            {
                // score points
                ConquerPoints += VisibleConquerUnits.Count * Time.deltaTime;   
            }
        }
        else if (VisibleOtherUnits.Count == 0 && VisibleConquerUnits.Count == 0)
        { ConquerPoints -= 1 * Time.deltaTime; }

        // clamp conquer points to zero
        ConquerPoints = Mathf.Max(ConquerPoints, 0.0f);

        // check if site was conquered
        if (ConquerPoints > ConquerPointsRequired)
        {
            conquerable.OnConquered(ConquererTeam);
            ConquerPoints = 0.0f;
            VisibleConquerUnits.Clear();
            VisibleOtherUnits.Clear();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) { return; }

        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            if (tm.Team == ConquererTeam)
            {
                if (!VisibleConquerUnits.Contains(tm))
                { VisibleConquerUnits.Add(tm); }
            }
            else if (tm.Team != conquerable.GetTeam() && VisibleConquerUnits.Count == 0)
            {
                if (!VisibleConquerUnits.Contains(tm))
                { VisibleConquerUnits.Add(tm); }
                
                conquerable.OnStartedConquering(tm.Team);
            }
            else if(tm.Team != ConquererTeam && ConquererTeam != -1)
            {
                if (!VisibleOtherUnits.Contains(tm))
                { VisibleOtherUnits.Add(tm); }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) { return; }
        
        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            if (tm.Team == ConquererTeam)
            {
                int conquererTeam = ConquererTeam;
                VisibleConquerUnits.Remove(tm);
                
                if (VisibleConquerUnits.Count == 0)
                {
                    conquerable.OnStoppedConquering(conquererTeam);
                    if (VisibleOtherUnits.Count > 0)
                    {
                        VisibleConquerUnits.AddRange(VisibleOtherUnits);
                        VisibleOtherUnits.Clear();
                        conquerable.OnStartedConquering(ConquererTeam);
                    }
                }
            }
            else
            { VisibleOtherUnits.Remove(tm); }
        }
    }
}
