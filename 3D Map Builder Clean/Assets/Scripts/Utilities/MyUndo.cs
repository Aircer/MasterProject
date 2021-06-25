using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUndo { 

    public HashSet<Vector3Int> lastIndexToPaint { get; set; }

    public PaintMode last_mode_paint { get; set; }

    public int last_pallet_index { get; set; }

    public bool noUndo { get; set; }

    public static MyUndo UpdateUndo(MyUndo undo, HashSet<Vector3Int> indexToPaint, PaintMode mode_paint, int pallet_index)
    {
        undo.lastIndexToPaint = indexToPaint;
        undo.last_mode_paint = mode_paint;
        undo.last_pallet_index = pallet_index;
        undo.noUndo = false;

        return undo;
    }
}
