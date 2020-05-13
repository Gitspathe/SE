using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace SE.World
{

    [Serializable]
    public class Marker
    {
        public Vector2 Position;
        public string ID;
        public string[] Tags;

        [JsonIgnore] public MarkerGraphicData GraphicData;

        public Marker(Vector2 position, string id, string[] tags, MarkerGraphicData graphicData = null)
        {
            Position = position;
            ID = id;
            Tags = tags;
            GraphicData = graphicData;
        }

        public Marker() { }

        public void SetGraphicData(MarkerGraphicData data)
        {
            GraphicData = data;
        }

    }

}
