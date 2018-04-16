using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

// This source file is for the implementation of the A* pathfinding / graph search algorithm.  Two classes are defined.  The first is a data type for nodes on the graph and the second is the actual implementation of the search built using the former data type.  Drawing functions for debugging exist too.


namespace LittleTiggy
{
    public class Node : IEquatable<Node>  //device graph node data type used in graph searchs
    {
        public ushort h_score { get; set; }
        public ushort g_score { get; set; }
        public ushort f_score
        {
            get
            {
                return (ushort)(h_score + g_score);
            }
        }


        public Vector2 position { get; set; }
        public Vector2 parent { get; set; } //parent node

        public Node(Vector2 Position, ushort H_Score, ushort G_Score)
        {
            g_score = G_Score;
            h_score = H_Score;
            position = Position;

        }

        public bool Equals(Node otherNode) //use X,Y coord of node for equality
        {
            if (this.position == otherNode.position)
                return true;

            return false;
        }

    }



    public class Pathfinder
    {

        List<Vector2> Path = new List<Vector2>();
        List<Vector2> DeletedNodes = new List<Vector2>();
        Animation DeletedNodesIdle;
        Animation DeletedNodesCurrentAnimation;

        static Texture2D environmentSheetTexture;
        Animation PathIdle;
        Animation PathCurrentAnimation;
        KeyboardState OldKeyboardState;

        public Pathfinder(GraphicsDevice graphicsDevice)
        {
            if (environmentSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/environmentSheet.png"))
                {
                    environmentSheetTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            PathIdle = new Animation();
            PathIdle.AddFrame(new Rectangle(32, 0, 16, 16), TimeSpan.FromSeconds(.25));

            PathCurrentAnimation = PathIdle;

            DeletedNodesIdle = new Animation();
            DeletedNodesIdle.AddFrame(new Rectangle(16, 0, 16, 16), TimeSpan.FromSeconds(.25));

            DeletedNodesCurrentAnimation = DeletedNodesIdle;

        }


        public void Update(GraphicsDevice graphicsDevice, EnvironmentBlock[] walls)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Z) && !OldKeyboardState.IsKeyDown(Keys.Z))
            {
                Vector2 source = new Vector2(0, 0);

                Vector2 destination = new Vector2(mainCharacter.X - (mainCharacter.X % 16), mainCharacter.Y - (mainCharacter.Y % 16));

                // Path = new Stack<Vector2>();

                DeletedNodes = new List<Vector2>();

                Path = Pathfind(source, destination, walls);
                //Pathfind(source, destination, walls);

            }

            OldKeyboardState = Keyboard.GetState();

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color tintColor = Color.White;


            var DeletedNodesSourceRectangle = DeletedNodesCurrentAnimation.CurrentRectangle;

            if (DeletedNodes != null)
            {
                foreach (Vector2 topLeftOfPathSquare in DeletedNodes)
                {
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, DeletedNodesSourceRectangle, tintColor);
                }
            }

            var sourceRectangle = PathCurrentAnimation.CurrentRectangle;

            if (Path != null)
            {
                /*
                for (int i = 0; i < Path.Count; i++)
                {
                    Vector2 topLeftOfPathSquare = Path.Pop();
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, sourceRectangle, tintColor);
                } */

                foreach (Vector2 topLeftOfPathSquare in Path)
                {
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, sourceRectangle, tintColor);
                }
            }
        }

        // Main A* pathfinding algorithm implementation

        public List<Vector2> Pathfind(Vector2 from, Vector2 destination, EnvironmentBlock[] walls)  
        {

            Node goalNode = new Node(destination, 0, 0);
            Node startNode = new Node(from, ManhattanDistance(from, destination), 0);

            List<Node> open = new List<Node>();                 //list of nodes
            List<Node> closed = new List<Node>();
            open.Add(startNode);                                //Add starting point

            while (open.Count > 0)
            {

                Node node = GetBestNode(open);                  // Get node with lowest F value

                if (node.position == goalNode.position)         // Goal reached
                {
                    return GetPath(node, closed, from);
                }
                open.Remove(node);

                closed.Add(node);

                List<Node> neighbours = GetNeighbours(node, walls, closed); //get all valid neighbour nodes; i.e. areas not taken up by walls or outside the play area 

                // DEBUG: following foreach is purely for visualisation of a*
                foreach (Node neighbour in neighbours)
                {
                    DeletedNodes.Add(neighbour.position);
                }

                foreach (Node neighbour in neighbours)
                {
                    ushort g_score = (ushort)(node.g_score + 16);
                    ushort h_score = ManhattanDistance(neighbour.position, goalNode.position);
                    ushort f_score = (ushort)(g_score + h_score);

                    if (!open.Contains(neighbour) || f_score < (neighbour.g_score + neighbour.h_score))
                    {
                        neighbour.parent = node.position;
                        neighbour.g_score = g_score;
                        neighbour.h_score = h_score;
                        if (!open.Contains(neighbour))
                        {
                            open.Add(neighbour);
                        }
                    }
                }
            }
            List<Vector2> emptyVectorList = new List<Vector2>();
            return emptyVectorList;
        }

        public ushort ManhattanDistance(Vector2 source, Vector2 destination)
        {
            return (ushort)Math.Floor(Math.Abs(source.X - destination.X) + (Math.Abs(source.Y - destination.Y)));
        }


        public List<Node> GetNeighbours(Node node, EnvironmentBlock[] walls, List<Node> existingNodes) //find all valid neighbours
        {
            List<Node> neighbourList = new List<Node>();


            // nodes to the left and right, above and below the current node are valid

            Node neighbourNode = new Node(new Vector2(node.position.X + 16, node.position.Y), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X - 16, node.position.Y), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X, node.position.Y + 16), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X, node.position.Y - 16), 999, 999);
            neighbourList.Add(neighbourNode);

            List<Node> neighbourListNoWalls = new List<Node>(neighbourList);

            // don't include any neighbour nodes if walls are already placed there.

            for (int i = 0; i < walls.Length; i++)
            {
                foreach (Node n in neighbourList)
                {
                    if (walls[i].X == n.position.X && walls[i].Y == n.position.Y)
                        neighbourListNoWalls.Remove(n);
                }
            }

            // don't include any neighbour nodes if those nodes have already been evaluated.

            foreach (Node existingNode in existingNodes)
            {
                foreach (Node n in neighbourList)
                {
                    if (existingNode.position == n.position)
                        neighbourListNoWalls.Remove(existingNode);
                }
                
            }

            // don't include any neighbour nodes if they go outside the play area.

            for (int i = 0; i < neighbourListNoWalls.Count; i++)
            {
                if (neighbourListNoWalls[i].position.X < 0 || neighbourListNoWalls[i].position.Y < 0 || neighbourListNoWalls[i].position.X > 512 || neighbourListNoWalls[i].position.Y > 512)
                {
                    neighbourListNoWalls.Remove(neighbourListNoWalls[i]);
                }
            }

            return neighbourListNoWalls;

        }

        //return node with lowest F score
        public Node GetBestNode(List<Node> nodes) 
        {
            Node nodeWithLowestFScore = new Node(new Vector2(0, 0), 999, 999);

            foreach (Node node in nodes)
            {
                if (node.g_score + node.h_score < nodeWithLowestFScore.g_score + nodeWithLowestFScore.h_score)
                {
                    nodeWithLowestFScore.g_score = node.g_score;
                    nodeWithLowestFScore.h_score = node.h_score;
                    nodeWithLowestFScore.parent = node.parent;
                    nodeWithLowestFScore.position = node.position;
                }
            }

            return nodeWithLowestFScore;
        }

        // Used to iterate over nodes starting with final node to generate a list of positions (path) a* found to navigate from src to destination
        public List<Vector2> GetPath(Node lastNode, List<Node> nodes, Vector2 fromPosition) 
        {

            List<Vector2> vectorList = new List<Vector2>();
            Node tempNode = new Node(new Vector2(0,0), 999, 999);
            bool foundStartPosition = false;
            DateTime timer = DateTime.Now;
            timer = timer.AddSeconds(0.5);

            foreach (Node node in nodes) // start with destination node and find it's parent
            {
                if (node.position == lastNode.parent)
                {
                    vectorList.Add(node.position);
                    tempNode = node;
                    nodes.Remove(node);
                    break;
                }
            }


            do
            {
                List<Node> cleanNodeList = new List<Node>(nodes);

                foreach (Node node in nodes)
                {
                    if (node.position == tempNode.parent)  // if we've found the previously found node's parent.
                    {
                        vectorList.Add(node.position);
                        tempNode = node;
                        cleanNodeList.Remove(node);

                        if (node.position == fromPosition)
                        {
                            foundStartPosition = true;
                            break;
                        }
                    }
                }

                nodes = cleanNodeList;

            } while (!foundStartPosition && timer.CompareTo(DateTime.Now) > 0);

            return vectorList;

        }
    }

}
