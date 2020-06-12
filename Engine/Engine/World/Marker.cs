using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace SE.World
{

    [Serializable]
    public class Marker
    {
        public Vector2 Position { get; set; }
        public string ID { get; set; }
        public string[] Tags { get; set; }

        [IgnoreDataMember] public MarkerGraphicData GraphicData { get; set; }

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
