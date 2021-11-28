//CREDIT TO https://github.com/davecusatis/A-Star-Sharp upon which we have modified/customized

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AStarSharp
{
    public class Node
    {
        // Change this depending on what the desired size is for each element in the grid
        public static int NODE_SIZE = 1; //was 32?
        public Node Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.x + NODE_SIZE / 2, Position.y + NODE_SIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        private int doorExtra = 1;
        private int destroyExtra = 4;

        List<List<Node>> Grid;
        int GridRows
        {
            get
            {
               return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End, bool destruction = false, int movement = 0, List<HoldableObject> weapons = null)
        {
            Node start = new Node(new Vector2((int)(Start.x / Node.NODE_SIZE), (int) (Start.y / Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.x / Node.NODE_SIZE), (int)(End.y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            List<Node> OpenList = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;
           
            // add start node to Open List
            OpenList.Add(start);

            while(OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach (Node n in adjacencies)
                {
                    int destroyCost = -1;
                    if (destruction && n.Walkable && n.Weight > 1)
                    {
                        List<HoldableObject> usableWeapons = new List<HoldableObject>();
                        int movesTaken = OpenList.Count - 1;
                        foreach (HoldableObject weapon in weapons) {
                            usableWeapons.Add(weapon);
                        }
                        usableWeapons.Sort((w1, w2) => w1.amount.CompareTo(w2.amount));
                        usableWeapons.Reverse();
                        if (usableWeapons.Count > 0)
                        {
                            float damage = usableWeapons[0].amount;
                            destroyCost = (int) Mathf.Max(0, Mathf.Ceil((n.Weight - 1) / damage) * movement - movesTaken);
                            Debug.Log("Destroy Cost: " + destroyCost);
                        }
                    }

                    bool walkable = n.Position == start.Position || n.Position == end.Position || (n.Walkable && n.Weight <= 1) || destroyCost >= 0;

                    if (!ClosedList.Contains(n) && !OpenList.Contains(n) && walkable)
                    {
                        n.Parent = current;
                        n.DistanceToTarget = Math.Abs(n.Position.x - end.Position.x) + Math.Abs(n.Position.y - end.Position.y);
                        n.Cost = (n.Weight == -1 ? doorExtra : (n.Weight > 1 ? destroyCost + destroyExtra : 0)) + 1 + n.Parent.Cost; //-1 signifies a door
                        OpenList.Add(n);
                        OpenList = OpenList.OrderBy(node => node.F).ToList();
                    }
                }
            }
            
            // construct path, if end was not closed return null
            if(!ClosedList.Exists(x => x.Position == end.Position))
            {
                Debug.Log("CAN'T FIND PATH");
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null) ;
            return Path;
        }

        public Stack<Node> FindPath(Vector2 Start, List<Vector2Int> Ends)
        {
            Node start = new Node(new Vector2((int)(Start.x / Node.NODE_SIZE), (int)(Start.y / Node.NODE_SIZE)), true);
            List<Node> ends = new List<Node>();
            foreach (Vector2 End in Ends)
            {
                ends.Add(new Node(new Vector2((int)(End.x / Node.NODE_SIZE), (int)(End.y / Node.NODE_SIZE)), true));
            }

            Stack<Node> Path = new Stack<Node>();
            List<Node> OpenList = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Add(start);

            Node endFound = null;
            while (OpenList.Count != 0 && endFound == null)
            {
                foreach (Node end in ends)
                {
                    current = OpenList[0];
                    OpenList.Remove(current);
                    ClosedList.Add(current);
                    adjacencies = GetAdjacentNodes(current);

                    foreach (Node n in adjacencies)
                    {

                        bool walkable = n.Position == start.Position || n.Position == end.Position || n.Walkable;

                        if (!ClosedList.Contains(n) && !OpenList.Contains(n) && walkable)
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.x - end.Position.x) + Math.Abs(n.Position.y - end.Position.y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(node => node.F).ToList();
                        }
                    }
                    if (ClosedList.Exists(x => x.Position == end.Position))
                    {
                        endFound = end;
                        break;
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Position == endFound.Position))
            {
                Debug.Log("CAN'T FIND PATH");
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = (int)n.Position.y;
            int col = (int)n.Position.x;

            if(row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if(row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if(col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if(col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}
