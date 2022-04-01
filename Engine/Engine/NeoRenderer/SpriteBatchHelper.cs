using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using System.Runtime.InteropServices;

namespace SE.NeoRenderer
{
    internal static unsafe class SpriteBatcherHelper
    {
        public static void Set(VertexPositionColorTexture* ptr, float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
        {
            VertexPositionColorTexture* vertexTL = (ptr + 0);
            VertexPositionColorTexture* vertexTR = (ptr + 1);
            VertexPositionColorTexture* vertexBL = (ptr + 2);
            VertexPositionColorTexture* vertexBR = (ptr + 3);

            vertexTL->Position.X = x + dx * cos - dy * sin;
            vertexTL->Position.Y = y + dx * sin + dy * cos;
            vertexTL->Position.Z = depth;
            vertexTL->Color = color;
            vertexTL->TextureCoordinate.X = texCoordTL.X;
            vertexTL->TextureCoordinate.Y = texCoordTL.Y;

            vertexTR->Position.X = x + (dx + w) * cos - dy * sin;
            vertexTR->Position.Y = y + (dx + w) * sin + dy * cos;
            vertexTR->Position.Z = depth;
            vertexTR->Color = color;
            vertexTR->TextureCoordinate.X = texCoordBR.X;
            vertexTR->TextureCoordinate.Y = texCoordTL.Y;

            vertexBL->Position.X = x + dx * cos - (dy + h) * sin;
            vertexBL->Position.Y = y + dx * sin + (dy + h) * cos;
            vertexBL->Position.Z = depth;
            vertexBL->Color = color;
            vertexBL->TextureCoordinate.X = texCoordTL.X;
            vertexBL->TextureCoordinate.Y = texCoordBR.Y;

            vertexBR->Position.X = x + (dx + w) * cos - (dy + h) * sin;
            vertexBR->Position.Y = y + (dx + w) * sin + (dy + h) * cos;
            vertexBR->Position.Z = depth;
            vertexBR->Color = color;
            vertexBR->TextureCoordinate.X = texCoordBR.X;
            vertexBR->TextureCoordinate.Y = texCoordBR.Y;
        }

        public static void Set(VertexPositionColorTexture* ptr, float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
        {
            VertexPositionColorTexture* vertexTL = (ptr + 0);
            VertexPositionColorTexture* vertexTR = (ptr + 1);
            VertexPositionColorTexture* vertexBL = (ptr + 2);
            VertexPositionColorTexture* vertexBR = (ptr + 3);

            vertexTL->Position.X = x;
            vertexTL->Position.Y = y;
            vertexTL->Position.Z = depth;
            vertexTL->Color = color;
            vertexTL->TextureCoordinate.X = texCoordTL.X;
            vertexTL->TextureCoordinate.Y = texCoordTL.Y;

            vertexTR->Position.X = x + w;
            vertexTR->Position.Y = y;
            vertexTR->Position.Z = depth;
            vertexTR->Color = color;
            vertexTR->TextureCoordinate.X = texCoordBR.X;
            vertexTR->TextureCoordinate.Y = texCoordTL.Y;

            vertexBL->Position.X = x;
            vertexBL->Position.Y = y + h;
            vertexBL->Position.Z = depth;
            vertexBL->Color = color;
            vertexBL->TextureCoordinate.X = texCoordTL.X;
            vertexBL->TextureCoordinate.Y = texCoordBR.Y;

            vertexBR->Position.X = x + w;
            vertexBR->Position.Y = y + h;
            vertexBR->Position.Z = depth;
            vertexBR->Color = color;
            vertexBR->TextureCoordinate.X = texCoordBR.X;
            vertexBR->TextureCoordinate.Y = texCoordBR.Y;
        }
    }

    // Was Pack = 1
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;
        
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration {
            get {
                return VertexDeclaration;
            }
        }

        static VertexPositionColorTexture()
        {
            var elements = new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
