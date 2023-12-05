using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConquerModule : NetworkBehaviour, ITeamMember
{
    public int Team // used as a wrapper around the parent object Team
    {
        get => TeamMember != null ? TeamMember.Team : -1;
        set { if (TeamMember != null) { TeamMember.Team = value; } }
    }
    [SerializeField] ITeamMember TeamMember = null;
    [SerializeField] int? ConquererTeam = null;
    [SerializeField] NetworkVariable<float> _ConquerPoints = new();
    [SerializeField] float ConquerPointsRequired = 50.0f;
    [SerializeField] List<Transform> VisibleConquerUnits = new();
    [SerializeField] List<Transform> VisibleOtherUnits = new();

    public VictoryPoint victoryPoint;

    public float ConquerPoints { get => _ConquerPoints.Value; set => _ConquerPoints.Value = value; }

    private IProgressBar ProgressBar = null;

    // public UnityAction<float> OnConquerPointsChanged;

    private void Start()
    {
        // initialize the team member (the object we will be setting Team)
        TeamMember = transform.parent?.GetComponent<ITeamMember>();
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

        // score points (note - one of those will always be 0)
        ConquerPoints += VisibleConquerUnits.Count * Time.deltaTime;
        ConquerPoints -= VisibleOtherUnits.Count * Time.deltaTime;
        if (VisibleOtherUnits.Count == 0 && VisibleConquerUnits.Count == 0)
        { ConquerPoints -= 1 * Time.deltaTime; }

        // clamp conquer points to zero
        ConquerPoints = Mathf.Max(ConquerPoints, 0.0f);

        // check if site wasn't conquered
        // note - this happens only if there no visible friendly or other units
        if (ConquerPoints > ConquerPointsRequired)
        {
            int team = ConquererTeam.Value;
            Team = team;
            ConquererTeam = -1;
            ConquerPoints = 0.0f;

            VisibleOtherUnits = VisibleConquerUnits;
            VisibleConquerUnits = new List<Transform>();
        
            if (victoryPoint) {
                victoryPoint.ConquerThisVP(team);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            if (tm.Team == ConquererTeam)
            {
                if (!VisibleConquerUnits.Contains(collision.transform))
                { VisibleConquerUnits.Add(collision.transform); }
            }
            else if (tm.Team != Team && VisibleConquerUnits.Count == 0)
            {
                ConquererTeam = tm.Team;
                if (!VisibleConquerUnits.Contains(collision.transform))
                { VisibleConquerUnits.Add(collision.transform); }
            }
            else
            {
                if (!VisibleOtherUnits.Contains(collision.transform))
                { VisibleOtherUnits.Add(collision.transform); }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            if (tm.Team == ConquererTeam)
            { VisibleConquerUnits.Remove(collision.transform); }
            else
            { VisibleOtherUnits.Remove(collision.transform); }
        }
    }
}
