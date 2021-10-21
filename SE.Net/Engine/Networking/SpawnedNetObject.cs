namespace SE.Engine.Networking
{
    public class SpawnedNetObject
    {
        public INetLogic NetLogic;
        public uint NetworkID;
        public string SpawnableID;
        public string Owner;

        public SpawnedNetObject(INetLogic netLogic, uint netID, string spawnID, string owner = "SERVER")
        {
            NetLogic = netLogic;
            NetworkID = netID;
            SpawnableID = spawnID;
            Owner = owner;
        }
    }
}
