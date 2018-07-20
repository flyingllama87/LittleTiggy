using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

// This source file is for the implementation of the A* pathfinding / graph search algorithm.  Two classes are defined.  The first is a data type for nodes on the graph and the second is the actual implementation of the search built using the former data type.  Drawing functions for debugging exist too.


namespace LittleTiggy
{

    enum NodeDirection { Up, Down, Left, Right, Unset }; // Used in path optomisation function

    public class Node : IEquatable<Node>  //device graph node data type used in graph search
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
        public Vector2 parent { get; set; } //parent node position

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

        public List<Vector2> PathToDraw = new List<Vector2>();
        List<Vector2> Path = new List<Vector2>();
        List<Vector2> DeletedNodes = new List<Vector2>(); // TO DO: Remove deleted nodes visualisation


        //The following exist for visualisation of path finding if debug is enabled
        static Texture2D environmentSheetTexture;
        Animation PathIdle;
        Animation PathCurrentAnimation;
        Animation DeletedNodesIdle; // TO DO: Remove deleted nodes visualisation
        Animation DeletedNodesCurrentAnimation; // TO DO: Remove deleted nodes visualisation

        public Vector2 from;
        public Vector2 destination;
        public EnvironmentBlock[] walls;

        int maxNodeCount; // This is used to calculate the maximum number of nodes the pathfinding algorithm should evaluate given it's environment.  If it exceeds this number, it has failed and needs to exit.

        public Pathfinder(GraphicsDevice graphicsDevice)
        {
            if (environmentSheetTexture == null)
            {
                using (var stream = TitleContainer.OpenStream("Content/EnvironmentSheet.png"))
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

            maxNodeCount = ((GameConstants.gameWidth / GameConstants.tileSize) * (GameConstants.gameHeight / GameConstants.tileSize));

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            Color tintColor = Color.White * 0.25f;

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
                
                for (int i = 0; i < Path.Count; i++)
                {
                    Vector2 topLeftOfPathSquare = Path[0];
                    Path.RemoveAt(0);
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, sourceRectangle, tintColor);
                } 

                foreach (Vector2 topLeftOfPathSquare in Path)
                {
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, sourceRectangle, tintColor);
                }
            }

            if (PathToDraw != null)
            {
                foreach (Vector2 topLeftOfPathSquare in PathToDraw)
                {
                    spriteBatch.Draw(environmentSheetTexture, topLeftOfPathSquare, sourceRectangle, tintColor);
                }
            } 
        }

        // Main A* pathfinding algorithm implementation

        public List<Vector2> Pathfind(Vector2 from, Vector2 destination, EnvironmentBlock[] walls)  // Used in IsRoutable to ensure generated levels are valid
        {
            
            if (from == destination)
            {
                return new List<Vector2>(); // Return empty vector list if asked to path find between two equal locations.
            }

            Node goalNode = new Node(destination, 0, 0);
            Node startNode = new Node(from, ManhattanDistance(from, destination), 0);

            List<Node> open = new List<Node>();                 //list of nodes
            List<Node> closed = new List<Node>();
            open.Add(startNode);                                //Add starting point


            while (open.Count > 0)
            {

                Node node = GetBestNode(open);                  // Get node with lowest F value

                open.Remove(node);
                closed.Add(node);

                if (node.position == goalNode.position)         // Goal reached
                {
                    return GetPath(node, closed, from);
                }

                List<Node> neighbours = GetNeighbours(node, walls, closed); //get all valid neighbour nodes; i.e. areas not taken up by walls or outside the play area 

                // DEBUG: following foreach is purely for visualisation of a*
                /*
                foreach (Node neighbour in neighbours)
                {
                    DeletedNodes.Add(neighbour.position);
                }*/

                Debug.WriteIf(open.Count > maxNodeCount, "Too many nodes allocated in pathfinding algorithm.  something went wrong");
                Debug.WriteIf(closed.Count > maxNodeCount, "Too many nodes allocated in pathfinding algorithm.  something went wrong");

                if (open.Count == 1 && closed.Count > maxNodeCount) //Something has gone wrong with the pathfinding algo and it needs to return as opposed to looping infinitely
                {
                    List<Vector2> emergencyEmptyVectorList = new List<Vector2>(); 
                    return emergencyEmptyVectorList;
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

        public List<Vector2> Pathfind() // Used for enemy background workers
        {

            if (from == destination)
            {
                return new List<Vector2>(); // Return empty vector list if asked to path find between two equal locations.
            }

            Node goalNode = new Node(destination, 0, 0);
            Node startNode = new Node(from, ManhattanDistance(from, destination), 0);

            List<Node> open = new List<Node>();                 //list of nodes
            List<Node> closed = new List<Node>();
            open.Add(startNode);                                //Add starting point

            Debug.WriteIf(open.Count > 1024, "Too many nodes allocated in pathfinding algorithm.  something went wrong");

            while (open.Count > 0)
            {

                Node node = GetBestNode(open);                  // Get node with lowest F value

                open.Remove(node);
                closed.Add(node);

                if (node.position == goalNode.position)         // Goal reached
                {
                    return GetPath(node, closed, from);
                }

                List<Node> neighbours = GetNeighbours(node, walls, closed); //get all valid neighbour nodes; i.e. areas not taken up by walls or outside the play area 

                // DEBUG: following foreach is purely for visualisation of a*
                /*
                foreach (Node neighbour in neighbours)
                {
                    DeletedNodes.Add(neighbour.position);
                }*/

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

        public bool IsRoutable(Vector2 from, Vector2 destination, EnvironmentBlock[] walls)
        {
            from.X = from.X - from.X % 16;
            from.Y = from.Y - from.Y % 16;

            destination.X = destination.X - destination.X % 16;
            destination.Y = destination.Y - destination.Y % 16;


            List<Vector2> vectorList;
            vectorList = Pathfind(from, destination, walls);
            if (vectorList.Contains(from) && vectorList.Contains(destination))
                return true;
            else
                return false;
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

            foreach (Node n in neighbourList)
            {
                if (n.position.X < 0 || n.position.Y < 0 || n.position.X > 512 || n.position.Y > 512)
                {
                    neighbourListNoWalls.Remove(n);
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
            timer = timer.AddSeconds(0.1);

            vectorList.Add(lastNode.position);

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

                if (timer.CompareTo(DateTime.Now) < 0)
                {
                    foundStartPosition = true;
                    break;
                }

            } while (!foundStartPosition);
            return vectorList;
        }
    }
}
