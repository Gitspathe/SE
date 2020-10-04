using System;

namespace SE.World
{
    public struct TileTemplate
    {
        public int TileID { get; set; }
        public bool IsNull => TileID == -1;

        private ITileAdditionalData additionalData;

        public byte[] SerializeAdditionalData()
        {
            return additionalData?.Serialize();
        }

        public void DeserializeAdditionalData<T>(byte[] bytes) where T : ITileAdditionalData, new()
        {
            // TODO: Pool T
            if (additionalData == null || additionalData.GetType() != typeof(T))
                additionalData = new T();

            additionalData.Restore(bytes);
        }

        public TileTemplate(int tileID, ITileAdditionalData additionalData = null)
        {
            TileID = tileID;
            this.additionalData = additionalData;
        }
    }
}
