using UnityEngine;

// Draws 3 meshes with the same material but with different colors.
[ExecuteInEditMode]
public class ColorCursor: MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    private MaterialPropertyBlock block;

    void OnEnable()
    {
        block = new MaterialPropertyBlock();
    }

    void SetMeshAspect(bool isCursor)
    {
        if(isCursor)
            block.SetColor("_Color", Color.blue);
        else
            block.SetColor("_Color", Color.white);

        Graphics.DrawMesh(mesh, this.transform.position, Quaternion.identity, material, 0, null, 0, block);
    }
}