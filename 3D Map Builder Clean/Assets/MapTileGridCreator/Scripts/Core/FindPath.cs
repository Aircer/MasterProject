using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapTileGridCreator.Core
{
    public static class Pathfinding
    {
        public static List<Waypoint> FindRouteTo(Waypoint Start, Waypoint End)
        {
            //We will store the path through a dictionary to keep track of where we came from to that point
            //and to keep track of the visited waypoints
            Dictionary<Waypoint, Waypoint> d = new Dictionary<Waypoint, Waypoint>();
            //first we save the root as visited in our dictionary
            d.Add(Start, null);

            //BFS to find the path with least nodes in between to our target
            Queue<Waypoint> q = new Queue<Waypoint>();
            q.Enqueue(Start);
            while (q.Count > 0)
            {
                Waypoint current = q.Dequeue();
                if (current == null)
                    continue;
                foreach (Waypoint e in current.outs)
                {
                    if (!d.ContainsKey(e))
                    {
                        q.Enqueue(e);
                        d.Add(e, current);
                    }
                }
            }

            //Now we have to translate from dictionary to list
            List<Waypoint> result = new List<Waypoint>();
            //start retrieving the path from the end
            Waypoint i = End;
            //until we find a node without origin (the starting node)
            while (i != null)
            {
                //insert at the beggining of the list
                result.Insert(0, i);
                //go to the next node
                if (d.ContainsKey(i))
                    i = d[i];
                else
                    return null;
            }

            return result;
        }

        public static List<Waypoint> FindRouteTo2(Waypoint start, Waypoint end)
        {
            List<Waypoint> openSet = new List<Waypoint>();
            HashSet<Waypoint> closedSet = new HashSet<Waypoint>();
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Waypoint currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == end)
                {
                    RetracePath(start, end);
                    return null;
                }

                foreach (Waypoint neighbour in currentNode.outs)
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                    float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, end);
                        neighbour.from = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            return openSet;
        }

        private static List<Waypoint> RetracePath(Waypoint startNode, Waypoint targetNode)
        {
            List<Waypoint> path = new List<Waypoint>();
            Waypoint currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.from;
            }

            path.Reverse();
            return path;
        }

        private static float GetDistance(Waypoint nodeA, Waypoint nodeB)
        {
            float dstX = Mathf.Abs(nodeA.transform.position.x - nodeB.transform.position.x);
            float dstY = Mathf.Abs(nodeA.transform.position.y - nodeB.transform.position.y);
            float dstZ = Mathf.Abs(nodeA.transform.position.z - nodeB.transform.position.z);

            return dstX + dstY + dstZ;
        }

    }
}