using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// The arena's ground: a flat square with every Trench cut out of it as a real hole.
// It rebuilds itself when the game starts, so moving a trench is enough for the floor.
// (Press Bake on the Arena's NavMeshSurface too, so enemies know where the holes are.)
// Keep this object unrotated and unscaled.
[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class ArenaFloor : MonoBehaviour
{
    [SerializeField] private float size = 240f;   // metres along each side, centred on this object

    private void Awake()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Holes")]
    public void Rebuild()
    {
        float half = size / 2f;

        // Where the holes are, in this object's space
        var holes = new List<Rect>();
        foreach (Trench trench in FindObjectsByType<Trench>(FindObjectsSortMode.None))
        {
            Rect r = trench.Footprint;
            Vector3 min = transform.InverseTransformPoint(new Vector3(r.xMin, 0f, r.yMin));
            Vector3 max = transform.InverseTransformPoint(new Vector3(r.xMax, 0f, r.yMax));
            holes.Add(Rect.MinMaxRect(min.x, min.z, max.x, max.z));
        }

        // Every hole edge becomes a grid line, so the floor is whole cells around the holes
        List<float> xs = GridLines(holes, half, true);
        List<float> zs = GridLines(holes, half, false);

        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        foreach (float z in zs)
        {
            foreach (float x in xs)
            {
                vertices.Add(new Vector3(x, 0f, z));
                uvs.Add(new Vector2(x / size + 0.5f, z / size + 0.5f));
            }
        }

        // Two triangles per cell, unless the middle of the cell is in a hole
        var triangles = new List<int>();
        int row = xs.Count;
        for (int j = 0; j < zs.Count - 1; j++)
        {
            for (int i = 0; i < xs.Count - 1; i++)
            {
                Vector2 middle = new Vector2((xs[i] + xs[i + 1]) / 2f, (zs[j] + zs[j + 1]) / 2f);
                if (holes.Exists(hole => hole.Contains(middle))) continue;

                int corner = j * row + i;   // the cell's bottom-left vertex
                triangles.Add(corner);
                triangles.Add(corner + row);
                triangles.Add(corner + 1);
                triangles.Add(corner + 1);
                triangles.Add(corner + row);
                triangles.Add(corner + row + 1);
            }
        }

        var mesh = new Mesh { name = "Arena Floor" };
        mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // The floor's edges plus every hole edge, sorted, without repeats
    private static List<float> GridLines(List<Rect> holes, float half, bool alongX)
    {
        var lines = new SortedSet<float> { -half, half };
        foreach (Rect hole in holes)
        {
            lines.Add(Snap(alongX ? hole.xMin : hole.yMin, half));
            lines.Add(Snap(alongX ? hole.xMax : hole.yMax, half));
        }
        return new List<float>(lines);
    }

    // Clamp to the floor and round to the millimetre, so nearly equal edges become one line
    private static float Snap(float value, float half)
    {
        return Mathf.Round(Mathf.Clamp(value, -half, half) * 1000f) / 1000f;
    }
}
