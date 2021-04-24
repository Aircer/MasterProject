using UnityEngine;

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
