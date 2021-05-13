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
                foreach (Waypoint e in current.GetNeighbors())
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

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == end)
                {
                    List<Waypoint> path = RetracePath(start, end);

                    for(int i = 0; i < path.Count; i++)
                    {
                        //path[i].colorDot = new Color(1.0f-((float)i / path.Count), 0, ((float)i /path.Count), 1f);
                        path[i].colorDot = Color.HSVToRGB(0.67f, ((float)i / path.Count), 0.76f);
                        path[i].inPath = true;
                    }

                    return;
                }

                foreach (Waypoint neighbour in currentNode.GetNeighbors())
                {
                    if (!neighbour.type.typeParams.blockPath || closedSet.Contains(neighbour) || !NeighbourReachable(neighbour, currentNode, maxJump)) continue;
                    float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    //Debug.Log(currentNode.name + " --> " + neighbour.name + " =" + GetDistance(currentNode, neighbour));
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, end);


                        float gCostFrom = Mathf.Infinity;
                        foreach (Waypoint from in neighbour.GetNeighbors())
                        {
                            if (openSet.Contains(from) && from.gCost < currentNode.gCost && from.gCost < gCostFrom && NeighbourReachable(neighbour, from, maxJump))
                            {
                                gCostFrom = from.gCost;
                                neighbour.from = from;
                            }
                        }

                        if (neighbour.from == null)
                            neighbour.from = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);

                        //Debug.Log(neighbour.from.name + " --> " + neighbour.name);
                    }
                }
            }

            Debug.Log("Path blocked");
            return;
        }

        public static void FindRouteTo3(Waypoint start, Waypoint end, Vector2 maxJump)
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

                foreach (Waypoint neighbour in currentNode.GetNeighbors())
                {
                    if ((neighbour.type != null && neighbour.type.typeParams.blockPath) || closedSet.Contains(neighbour) || !NeighbourReachable(neighbour, currentNode, maxJump)) continue;

                    float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        if (newMovementCostToNeighbour > maxGCost)
                            maxGCost = newMovementCostToNeighbour;
                        neighbour.from = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);

                        //Debug.Log(neighbour.from.key + " --> " + neighbour.key);
                    }
                }
            }

            List<Waypoint> path = RetracePath(start, end);

            if (path == null)
            {
                Debug.Log("Path blocked");
            }
            else
            {
                for (int i = 0; i < path.Count; i++)
                {
                    //path[i].colorDot = new Color(1.0f-((float)i / path.Count), 0, ((float)i /path.Count), 1f);
                    path[i].colorDot = Color.HSVToRGB(0.67f, ((float)i / path.Count), 0.76f);
                    path[i].inPath = true;
                    path[i].inPathFrom = path[i].from;
                }
            }

            /*for (int i = 0; i < closedSet.Count; i++)
            {
                //closedSet[i].colorDot = new Color(1.0f-(float)closedSet[i].gCost / maxGCost, 0.0f, (float)closedSet[i].gCost/ maxGCost, 1f);
                if (!InAir(closedSet[i]))
                {
                    closedSet[i].colorDot = Color.HSVToRGB(0.67f, (float)closedSet[i].gCost / maxGCost, 0.76f);
                    closedSet[i].showFlood = true;
                }
            }*/

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

                foreach (Waypoint neighbour in currentNode.GetNeighbors())
                {
                    if ((neighbour.type != null && neighbour.type.typeParams.blockPath) || closedSet.Contains(neighbour) || !NeighbourReachable(neighbour, currentNode, maxJump)) continue;

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

            for (int i = 1; i < closedSet.Count; i++)
            {
                //closedSet[i].colorDot = new Color(1.0f-(float)closedSet[i].gCost / maxGCost, 0.0f, (float)closedSet[i].gCost/ maxGCost, 1f);
                if (!InAir(closedSet[i]))
                {
                    closedSet[i].colorDot = Color.HSVToRGB(0.67f, (float)closedSet[i].gCost / maxGCost, 0.76f);
                    closedSet[i].inPath = true;
                    closedSet[i].inPathFrom = closedSet[i].from;
                }
            }

            return;
        }

        private static bool NeighbourReachable(Waypoint tryJump, Waypoint current, Vector2 max)
        {
            Vector2 dist = new Vector2(0.0f,0.0f);
            Waypoint startJump = current;
            List<Waypoint> waypointsJump = new List<Waypoint>
            {
                startJump,
                tryJump
            };

            if (!InAir(tryJump) && !InAir(startJump))
                return true;

            while (InAir(startJump))
            {
                if (startJump.from != null)
                {
                    startJump = startJump.from;
                    waypointsJump.Insert(0, startJump);
                }

                if (startJump.from == null)
                    break;
            }

            for (int i = 0; i < waypointsJump.Count-1; i++)
            {
                dist.x += Mathf.Sqrt(Mathf.Pow(Mathf.Abs(waypointsJump[i].key.x - waypointsJump[i+1].key.x),2) + Mathf.Pow(Mathf.Abs(waypointsJump[i].key.z - waypointsJump[i+1].key.z), 2));
                dist.y += Mathf.Abs(waypointsJump[i].key.y - waypointsJump[i + 1].key.y);
            }

            //Debug.Log(startJump.name + " --> " + tryJump.name + " : " + dist + " check: " + Mathf.Pow(dist.x, 2) / Mathf.Pow(max.x + 1, 2));

            if (startJump.key.y - tryJump.key.y < 0)
                return Mathf.Pow(dist.x, 2) / Mathf.Pow(max.x + 1,2) + Mathf.Pow(dist.y,2) / Mathf.Pow(max.y+1,2) < 1;
            else
                return Mathf.Pow(dist.x,2) / Mathf.Pow(max.x + 1,2) < 1;
        }

        private static List<Waypoint> RetracePath(Waypoint startNode, Waypoint targetNode)
        {
            List<Waypoint> path = new List<Waypoint>();
            Waypoint currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                if (currentNode.from == null)
                    return null;
                else
                    currentNode = currentNode.from;
            }

            path.Reverse();
            return path;
        }

        private static bool InAir(Waypoint w)
        {
            if (w.GetUpNeighbor() != null && w.GetDownNeighbor() != null && w.GetDownNeighbor().type && w.GetDownNeighbor().type.typeParams.ground)
                return false;
            else
                return true;
        }

        private static List<Waypoint> RetracePath2(Waypoint startNode, Waypoint targetNode, Vector2 maxJump)
        {
            List<Waypoint> path = new List<Waypoint>();
            Waypoint currentNode = targetNode;
            Waypoint oldNode = currentNode;
            path.Add(currentNode);

            int i = 0;
            while (currentNode != startNode && i < 10000)
            {
                float currentGCost = Mathf.Infinity;
                foreach (Waypoint neighbour in currentNode.GetNeighbors()) 
                {
                    if ((neighbour.gCost != 0 && neighbour.gCost < currentGCost) || neighbour == startNode)
                    {
                        currentGCost = neighbour.gCost;
                        currentNode = neighbour;
                    }
                }

                if (oldNode == currentNode)
                    return null;
                else
                    oldNode.from = currentNode;

                path.Add(currentNode);
                oldNode = currentNode;
                i++;
            }

            //path.Reverse();
            return path;
        }

        private static float GetDistance(Waypoint nodeA, Waypoint nodeB)
        {
            float dstX = Mathf.Pow(Mathf.Abs(nodeA.key.x - nodeB.key.x),2);
            float dstY = Mathf.Pow(Mathf.Abs(nodeA.key.y - nodeB.key.y),2);
            float dstZ = Mathf.Pow(Mathf.Abs(nodeA.key.z - nodeB.key.z),2);

            return Mathf.Sqrt(dstX + dstY + dstZ);
        }

       
    }
}