using UnityEngine;
using System.Collections.Generic;

public class MeshSplitter : MonoBehaviour
{
    public Material newMaterial;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.mesh;
                    int[] triangles = mesh.triangles;
                    Vector3[] vertices = mesh.vertices;
                    List<Vector3> newVertices = new List<Vector3>();
                    List<int> newTriangles = new List<int>();

                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        if (IsTriangleHit(triangles, i, hit.triangleIndex))
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                int vertexIndex = triangles[i + j];
                                newVertices.Add(vertices[vertexIndex]);
                                newTriangles.Add(newVertices.Count - 1);
                            }
                            break;
                        }
                    }

                    CreateNewMesh(newVertices, newTriangles);
                }
            }
        }
    }

    bool IsTriangleHit(int[] triangles, int index, int hitIndex)
    {
        return index / 3 == hitIndex;
    }

    void CreateNewMesh(List<Vector3> vertices, List<int> triangles)
    {
        GameObject newObj = new GameObject("Separated Mesh");
        newObj.transform.position = Vector3.zero;
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.RecalculateNormals();

        MeshFilter meshFilter = newObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newObj.AddComponent<MeshRenderer>();

        meshFilter.mesh = newMesh;
        meshRenderer.material = newMaterial != null ? newMaterial : new Material(Shader.Find("Standard"));
    }
}
