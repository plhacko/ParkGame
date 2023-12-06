namespace NavMeshPlus.Extensions.Units
{
    public interface IConquerable
    {
        public void OnStartedConquering(int team);
        
        public void OnConquered(int team);
        
        public int GetTeam();
    }
}