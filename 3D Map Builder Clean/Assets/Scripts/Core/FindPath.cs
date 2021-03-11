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

        public static void FindRouteTo2(Waypoint start, Waypoint end, Vector2 maxJump)
        {
            List<Waypoint> openSet = new List<Waypoint>();
            HashSet<Waypoint> closedSet = new HashSet<Waypoint>();
            Waypoint last = start; 
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Waypoint currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    //Debug.Log("LAST: " + last.name + ", POS: " + openSet[i]);
                    if (openSet[i].hCost < currentNode.hCost || (openSet[i].hCost == currentNode.hCost && openSet[i].gCost < currentNode.gCost))
                    {
                        currentNode = openSet[i];
                    }
                }
                last = currentNode; 
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == end)
                {
                    List<Waypoint> path = RetracePath(start, end);

                    for(int i = 0; i < path.Count; i++)
                    {
                        //path[i].colorDot = new Color(1.0f-((float)i / path.Count), 0, ((float)i /path.Count), 1f);
                        path[i].colorDot = Color.HSVToRGB(0.67f, ((float)i / path.Count), 0.76f);
                        path[i].SetShow(true);
                    }
                    return;
                }

                foreach (Waypoint neighbour in currentNode.outs)
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour) || !neighbourReachable(neighbour, currentNode, maxJump)) continue;

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

            Debug.Log("Path blocked");
            return;
        }

        public static void FloodFill(Waypoint start, Vector2 maxJump)
        {
            List<Waypoint> openSet = new List<Waypoint>();
            List<Waypoint> closedSet = new List<Waypoint>();
            float maxGCost = 0; 
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Waypoint currentNode = openSet[0];

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (Waypoint neighbour in currentNode.outs)
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour) || !neighbourReachable(neighbour, currentNode, maxJump)) continue;

                    float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        if(newMovementCostToNeighbour > maxGCost)
                            maxGCost = newMovementCostToNeighbour;
                        neighbour.from = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            for (int i = 0; i < closedSet.Count; i++)
            {
                //closedSet[i].colorDot = new Color(1.0f-(float)closedSet[i].gCost / maxGCost, 0.0f, (float)closedSet[i].gCost/ maxGCost, 1f);
                if (closedSet[i].inAir == false)
                {
                    closedSet[i].colorDot = Color.HSVToRGB(0.67f, (float)closedSet[i].gCost / maxGCost, 0.76f);
                    closedSet[i].SetShowFlood(true);
                }
            }
            return;
        }


        private static bool neighbourReachable(Waypoint tryJump, Waypoint current, Vector2 max)
        {
            float d;
            Waypoint startJump = current;

            if (!tryJump.inAir)
                return true;

            while (startJump.inAir)
            {
                if (startJump.from)
                    startJump = startJump.from;

                if (startJump.from == null)
                    continue;
            }
            //Debug.Log(startJump.name + "--> " + tryJump.name);
            if(startJump.transform.position.y - tryJump.transform.position.y < 0)
                d = (Mathf.Pow(startJump.transform.position.x - tryJump.transform.position.x, 2) / (Mathf.Pow(max.x + 1, 2) + 0.001f)) + (Mathf.Pow(Mathf.Abs(startJump.transform.position.y - tryJump.transform.position.y), 2) / (Mathf.Pow(max.y, 2) + 0.001f)) + (Mathf.Pow(Mathf.Abs(startJump.transform.position.z - tryJump.transform.position.z), 2) / (Mathf.Pow(max.x + 1, 2) + 0.001f));
            else
                d = (Mathf.Pow(Mathf.Abs(startJump.transform.position.x - tryJump.transform.position.x), 2)/ (Mathf.Pow(max.x + 1, 2) + 0.001f)) + (Mathf.Pow(Mathf.Abs(startJump.transform.position.z - tryJump.transform.position.z), 2) / (Mathf.Pow(max.x + 1, 2) + 0.001f));

            if (d > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
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