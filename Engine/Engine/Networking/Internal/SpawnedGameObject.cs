using SE.Common;

namespace SE.Networking.Internal
{
    internal class SpawnedGameObject
    {
        public GameObject GameObject;
        public uint NetworkID;
        public string SpawnableID;
        public string Owner;

        public SpawnedGameObject(GameObject go, uint netID, string spawnID, string owner = "SERVER")
        {
            GameObject = go;
            NetworkID = netID;
            SpawnableID = spawnID;
            GameObject.NetIdentity.SpawnedGameObject = this;
            Owner = owner;
        }
    }
}
