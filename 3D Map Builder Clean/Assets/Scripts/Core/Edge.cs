using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapTileGridCreator.Core
{
    public class Edge 
    {
        public Waypoint originWaypoint;
        public Waypoint destinationWaypoint;
        public float travelDifficulty;
    }
}
