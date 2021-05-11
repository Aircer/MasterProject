using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Colors used in editor and gizmo for default colors depending of an arbitrary convention.
/// </summary>
public static class Constants
{
	public static Vector3Int UNDEFINED_POSITION = new Vector3Int(-1, -1, -1);
}

public enum PaintMode
{
	Single, Erase, Eyedropper, SetPathfindingWaypoint
};

public enum CellState
{
	Painted, Erased, Active, Inactive, Sleep
};

public enum PathfindingState
{
	A_Star, Floodfill
};

public struct EvolutionaryAlgoParams
{
    public float mutationRate;
    public int population;
    public int generations;
	public int elitism;
	public int skipMutations;
}

public struct WaypointParams
{
	public int type;
	public Vector3 rotation;
	public Vector3Int basePos;
	public bool baseType;
}

public struct TypeParams
{
	public Vector3Int size;
	public bool ground;
	public bool blockPath;
	public bool wall;
	public bool floor;
}

public struct Phenotype
{
	public HashSet<Wall> walls_x;
	public HashSet<Wall> walls_z;
	public HashSet<Vector3Int> blocksSolo; 
}

public struct Wall
{
	public HashSet<Vector3Int> indexes;
	public int position; 
}
