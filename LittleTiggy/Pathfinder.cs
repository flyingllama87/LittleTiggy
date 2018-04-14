using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace LittleTiggy
{
    public class Node : IEquatable<Node>
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
        public Vector2 parent { get; set; }

        public Node(Vector2 Position, ushort H_Score, ushort G_Score)
        {
            g_score = G_Score;
            h_score = H_Score;
            position = Position;

        }

        public bool Equals(Node otherNode)
        {
            if (this.position == otherNode.position)
                return true;

            return false;
        }

    }

    public class Pathfinder
    {

        public Stack<Vector2> Pathfind(Vector2 from, Vector2 destination, EnvironmentBlock[] walls)
        {

            Node goalNode = new Node(destination, 0, 0);
            Node startNode = new Node(from, ManhattanDistance(from, destination), 0);

            List<Node> open = new List<Node>();            //list of nodes
            List<Node> closed = new List<Node>();
            open.Add(startNode);                //Add starting point

            while (open.Count > 0)
            {

                Node node = GetBestNode(open);                   //Get node with lowest F value

                if (node.position == goalNode.position)
                {
                    //Debug.Log("Goal reached");
                    return GetPath(node, closed, from);
                }
                open.Remove(node);
                //Node nodeToRemove = open.Find(n => n.position == node.position);
                //open.Remove(nodeToRemove);

                closed.Add(node);

                List<Node> neighbors = GetNeighbours(node, walls);

                foreach (Node n in neighbors)
                {
                    ushort g_score = (ushort)(node.g_score + 16);
                    ushort h_score = ManhattanDistance(n.position, goalNode.position);
                    ushort f_score = (ushort)(g_score + h_score);

                    if (closed.Contains(n) && f_score >= (n.g_score + n.h_score))
                        continue;

                    if (!open.Contains(n) || f_score < (n.g_score + n.h_score))
                    {
                        n.parent = node.position;
                        n.g_score = g_score;
                        n.h_score = h_score;
                        if (!open.Contains(n))
                        {
                            // map_data[n.position.x, n.position.y] = 4; // ?? Not sure what the dev is trying to do here
                            open.Add(n);
                        }
                    }
                }
            }
            Stack<Vector2> emptyVectorStack = new Stack<Vector2>();
            return emptyVectorStack;
        }

        public ushort ManhattanDistance(Vector2 source, Vector2 destination)
        {
            return (ushort)Math.Floor(Math.Abs(source.X - destination.X) + (Math.Abs(source.Y - destination.Y)));
        }


        public List<Node> GetNeighbours(Node node, EnvironmentBlock[] walls)
        {
            List<Node> neighbourList = new List<Node>();


            Node neighbourNode = new Node(new Vector2(node.position.X + 16, node.position.Y), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X - 16, node.position.Y), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X, node.position.Y + 16), 999, 999);
            neighbourList.Add(neighbourNode);

            neighbourNode = new Node(new Vector2(node.position.X, node.position.Y - 16), 999, 999);
            neighbourList.Add(neighbourNode);

            List<Node> neighbourListNoWalls = new List<Node>(neighbourList);


            for (int i = 0; i < walls.Length; i++)
            {
                foreach (Node n in neighbourList)
                {
                    if (walls[i].X == n.position.X && walls[i].Y == n.position.Y)
                        neighbourListNoWalls.Remove(n);
                }
            }

            for (int i = 0; i < neighbourListNoWalls.Count; i++)
            {
                if (neighbourListNoWalls[i].position.X < 0 || neighbourListNoWalls[i].position.Y < 0 || neighbourListNoWalls[i].position.X > 512 || neighbourListNoWalls[i].position.Y > 512)
                {
                    neighbourListNoWalls.Remove(neighbourListNoWalls[i]);
                }
            }

            return neighbourListNoWalls;

        }


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


        public Stack<Vector2> GetPath(Node lastNode, List<Node> nodes, Vector2 fromPosition)
        {

            Stack<Vector2> vectorStack = new Stack<Vector2>();
            Node tempNode = new Node(new Vector2(0,0), 999, 999);
            bool foundStartPosition = false;

            foreach (Node node in nodes)
            {
                if (node.position == lastNode.parent)
                {
                    vectorStack.Push(node.position);
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
                    if (node.position == tempNode.parent)
                    {
                        vectorStack.Push(node.position);
                        tempNode = node;
                        //cleanNodeList.Remove(node);

                        if (node.position == fromPosition)
                        {
                            foundStartPosition = true;
                            break;
                        }
                    }
                }

                nodes = cleanNodeList;

            } while (!foundStartPosition);

            return vectorStack;

        }
    }

}



/*
 * ----
MY A* ALGO:
---

	void PathFinder(Vector2 from, Vector2 destination)
	{

    goalNode = new Node(destination, 0, 0);
    startNode = new Node(from, 0, ManhattanDistance(from, destination));

    open = new List<Node>();            //list of nodes
    closed = new List<Node>();
    open.Add(startNode);                //Add starting point

    while(open.Count > 0) {

        node = getBestNode();                   //Get node with lowest F value
        if(node.position == goalNode.position) {
            Debug.Log("Goal reached");
            getPath(node);
            break;
        }
        open.Remove(node);
        closed.Add(node);

        List<Node> neighbors = getNeighbors(node);
        foreach(Node n in neighbors) {
            float g_score = node.G + 1;
            float h_score = ManhattanDistance(n.position, goalNode.position);
            float f_score = g_score + h_score;

            if(isValueInList(n, closed) && f_score >= n.F) 
                continue;

            if(!isValueInList(n, open) || f_score < n.F) {
                n.parent = node;
                n.G = g_score;
                n.H = h_score;
                if(!isValueInList(n, open)) {
                    // map_data[n.position.x, n.position.y] = 4; // ?? Not sure what the dev is trying to do here
                    open.Add(n);
                }
            }
        }
    }
	}


----

Functions to implement:

ushort ManhattanDistance(source, destination)
list<node> GetNeighbours(node)
node GetBestNode(); //node with lowest fscore
getPath (Node) 


Node class:

Node (Position, h_score, g_score)
Node (Vector2, ushort, ushort)

Node properties:

.Position (vector2) 
.Parent (vector2)
.G_Score
.H_Score
*/
