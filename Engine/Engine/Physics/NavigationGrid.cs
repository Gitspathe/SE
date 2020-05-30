using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Components;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Physics
{

    /// <summary>
    /// Grid used for path-finding.
    /// </summary>
    public class NavigationGrid : IAssetConsumer
    {
        public AssetConsumer AssetConsumer { get; } = new AssetConsumer();

        public LayerType[] Layers;
        public int NodeSize, NodesX, NodesY;
        private Node[][] nodes;

        /// <summary>
        /// Draws a debug overlay for the navigation grid.
        /// </summary>
        /// <param name="batch">Sprite-batch to draw into.</param>
        public void DrawDebug(Camera2D camera, SpriteBatch batch)
        {
            SpriteTexture st = AssetManager.Get<SpriteTexture>(this, "floor");
            for (int x = 0; x < nodes.Length; x++) {
                for (int y = 0; y < nodes[x].Length; y++) {
                    if (nodes[x][y].Cost < 255)
                        continue;

                    batch.Draw(st.Texture, new Microsoft.Xna.Framework.Vector2((x * NodeSize) - camera.Position.X, (y * NodeSize) - camera.Position.Y), st.SourceRectangle,
                        new Color(255, 0, 0, 70), 0, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Vector2.One, SpriteEffects.None, 1.0f);    
                }
            }
        }

        /// <summary>Creates a new NavigationGrid instance.</summary>
        /// <param name="nSize">Node size.</param>
        /// <param name="worldSize">World size.</param>
        /// <param name="layers">Physics layers applicable.</param>
        public NavigationGrid(int nSize, Rectangle worldSize, LayerType[] layers)
        {
            Layers = layers;
            NodeSize = nSize;
            NodesX = worldSize.Width / NodeSize;
            NodesY = worldSize.Height / NodeSize;
            nodes = new Node[NodesX][];
            for (int x = 0; x < NodesX; x++) {
                nodes[x] = new Node[NodesY];
                for (int y = 0; y < NodesY; y++) {
                    nodes[x][y] = new Node(x,y,0);
                }
            }
        }

        /// <summary>
        /// Updates the traversal cost of nodes within a given rectangle.
        /// </summary>
        /// <param name="rect">Rectangle region of nodes to update.</param>
        /// <param name="cost">New traversal cost.</param>
        public void Insert(Rectangle rect, byte cost = 255)
        {
            rect = WorldToGridSpace(rect);
            if (rect == Rectangle.Empty)
                return;

            for (int xLoop = rect.X; xLoop < rect.X+rect.Width; xLoop++) {
                for (int yLoop = rect.Y; yLoop < rect.Y+rect.Height; yLoop++) {
                    nodes[xLoop][yLoop].Cost = cost;
                }
            }
        }

        /// <summary>
        /// Resets the traversal cost of nodes within a specific region to zero.
        /// </summary>
        /// <param name="rect">Rectangle region of nodes to update.</param>
        public void Remove(Rectangle rect)
        {
            rect = WorldToGridSpace(rect);
            if (rect == Rectangle.Empty)
                return;

            for (int xLoop = rect.X; xLoop < rect.X+rect.Width; xLoop++) {
                for (int yLoop = rect.Y; yLoop < rect.Y+rect.Height; yLoop++) {
                    nodes[xLoop][yLoop].Cost = 0;
                }
            }
        }

        private Rectangle WorldToGridSpace(Rectangle rect)
        {
            if (rect.X < 1) rect.X = 1;
            if (rect.Y < 1) rect.Y = 1;
            int x = rect.X / NodeSize;
            int y = rect.Y / NodeSize;
            int w = rect.Width / NodeSize;
            int h = rect.Height / NodeSize;
            if (w < 1) w = 1;
            if (h < 1) h = 1;
            if (x + w > NodesX || x + h > NodesY) {
                return Rectangle.Empty;
            }
            return new Rectangle(x,y,w,h);
        }

        private Vector2 WorldToGridSpace(Vector2 pos)
        {
            if (pos.X < 1) pos.X = 1;
            if (pos.Y < 1) pos.Y = 1;
            float x = pos.X / NodeSize;
            float y = pos.Y / NodeSize;
            return new Vector2(x,y);
        }

        private Vector2 GridToWorldSpace(Vector2 pos)
        {
            float x = pos.X * NodeSize;
            float y = pos.Y * NodeSize;
            return new Vector2(x,y);
        }

        /// <summary>
        /// Calculates and returns the shortest path from a starting point to a destination point using an A* algorithm.
        /// </summary>
        /// <param name="s">Starting point.</param>
        /// <param name="d">Destination point.</param>
        /// <returns>Stack of nodes representing the path.</returns>
        public Stack<Node> FindPath(Vector2 s, Vector2 d)
        {
            s = WorldToGridSpace(s);
            d = WorldToGridSpace(d);
            if (NodeExistsAtPoint((int)d.X, (int)d.Y) && !nodes[(int)d.X][(int)d.Y].IsWalkable())
                return null;

            Node start = new Node((int)s.X, (int)s.Y, 0);
            Node end = new Node((int)d.X, (int)d.Y, 0);
            Node currentNode = start;

            // TODO: Make NonAlloc version of this function when done, to stop allocating lists into memory.
            Stack<Node> path = new Stack<Node>();
            List<Node> openList = new List<Node> { start };
            List<Node> closedList = new List<Node>();
            List<Node> adjacent;
            while (openList.Count != 0 && !closedList.Exists(n => (n.X == end.X && n.Y == end.Y))) {
                currentNode = openList[0];
                closedList.Add(currentNode);
                openList.Remove(currentNode);
                adjacent = GetAdjacent(currentNode);
                for (int i = 0; i < adjacent.Count; i++) {
                    Node n = adjacent[i];
                    if (!n.IsWalkable() || closedList.Contains(n) || openList.Contains(n))
                        continue;

                    n.Info = new NodeInfo();
                    n.Info.Parent = currentNode;
                    n.Info.DistanceToTarget = Math.Abs(n.X - end.X) + Math.Abs(n.Y - end.Y);
                    n.Info.FinalCost = 1 + n.Info.Parent.Cost;
                    openList.Add(n);
                }
                openList.Sort((x, y) => x.Info.F.CompareTo(y.Info.F));
            }
            if (!closedList.Exists(x => (x.X == end.X && x.Y == end.Y)))
                return null;

            Node tmp = closedList[closedList.IndexOf(currentNode)];
            while (tmp?.Info != null) { // && tmp.info.parent != start) {
                path.Push(tmp);
                tmp = tmp.Info.Parent;
            }
            return path;
        }

        private List<Node> GetAdjacent(Node n)
        {
            List<Node> adjacent = new List<Node>();
            int x = n.X;
            int y = n.Y;
            bool canWalkN = IsWalkable(x, y + 1);
            bool canWalkE = IsWalkable(x + 1, y);
            bool canWalkS = IsWalkable(x, y - 1);
            bool canWalkW = IsWalkable(x - 1, y);
            bool canWalkNE = canWalkN && canWalkE && IsWalkable(x + 1, y + 1);
            bool canWalkNW = canWalkN && canWalkW && IsWalkable(x - 1, y + 1);
            bool canWalkSE = canWalkS && canWalkE && IsWalkable(x + 1, y - 1);
            bool canWalkSW = canWalkS && canWalkW && IsWalkable(x - 1, y - 1);
            if (canWalkN) {
                adjacent.Add(nodes[x][y + 1]);
            }
            if (canWalkNE) {
                adjacent.Add(nodes[x + 1][y + 1]);
            }
            if (canWalkE) {
                adjacent.Add(nodes[x + 1][y]);
            }
            if (canWalkSE) {
                adjacent.Add(nodes[x + 1][y - 1]);
            }
            if (canWalkS) {
                adjacent.Add(nodes[x][y - 1]);
            }
            if (canWalkSW) {
                adjacent.Add(nodes[x - 1][y - 1]);
            }
            if (canWalkW) {
                adjacent.Add(nodes[x - 1][y]);
            }
            if (canWalkNW) {
                adjacent.Add(nodes[x - 1][y + 1]);
            }

            return adjacent;
        }

        private bool IsWalkable(int x, int y)
        {
            return NodeExistsAtPoint(x, y) && nodes[x][y].IsWalkable();
        }

        private bool NodeExistsAtPoint(int x, int y)
        {
            return x <= NodesX && y <= NodesY && x > 0 && y > 0 && nodes[x][y] != null;
        }

        /// <summary>
        /// Gets the world position of a specific node on the navigation grid.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <returns>World position of node.</returns>
        public Vector2 GetPosition(Node n)
        {
            return new Vector2((n.X * NodeSize)+(NodeSize/2), (n.Y * NodeSize)+(NodeSize/2));
        }

        /// <summary>
        /// Represents a point on a navigation grid.
        /// </summary>
        public class Node
        {
            /// <summary>Position in grid array coordinates.</summary>
            public int X, Y;

            /// <summary>How expensive it is to traverse this node.</summary>
            public byte Cost;

            /// <summary>Additional data. May be NULL.</summary>
            public NodeInfo Info;

            /// <summary>Creates a new node instance.</summary>
            /// <param name="x">X position in grid array coordinates.</param>
            /// <param name="y">Y position in grid array coordinates.</param>
            /// <param name="cost">How expensive it is to traverse this node.</param>
            public Node(int x, int y, byte cost)
            {
                X = x;
                Y = y;
                Cost = cost;
            }

            /// <summary>
            /// Checks if a node allows for traversal.
            /// </summary>
            /// <returns>True if the node is walkable.</returns>
            public bool IsWalkable()
            {
                return Cost < 255;
            }
        }

        /// <summary>
        /// Contains additional data for a node.
        /// </summary>
        public class NodeInfo
        {
            /// <summary>This node's parent node.</summary>
            public Node Parent;

            /// <summary>Current distance from the destination.</summary>
            public int DistanceToTarget;

            /// <summary>How expensive it is to traverse this node.</summary>
            public int FinalCost;

            /// <summary>How valid the node is for traversal.</summary>
            public float F {
                get {
                    if (DistanceToTarget != -1 && FinalCost != -1)
                        return DistanceToTarget + FinalCost;

                    return -1;
                }
            }
        }
    }

}
