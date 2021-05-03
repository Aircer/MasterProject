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
}

public struct WaypointParams
{
	public CellInformation type;
	public Vector3 rotation;
	public Vector3Int basePos;
	public bool baseType;
}

public struct Phenotype
{
	public List<Wall> walls_x;
	public List<Wall> walls_z;
}

public struct Wall
{
	public List<Vector3Int> indexes;
	public int position; 
	public List<Wall> neighbors;
}