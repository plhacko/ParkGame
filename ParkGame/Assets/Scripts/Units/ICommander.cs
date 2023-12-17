using Unity.Netcode;

public interface ICommander : ITeamMember
{

    public void ReportFollowing(NetworkObjectReference networkObjectReference);
    public void ReportUnfollowing(NetworkObjectReference networkObjectReference);
    public enum SoldierMovements { Free, Circle, Box };

    public Formation.FormationType GetFormation();
}
