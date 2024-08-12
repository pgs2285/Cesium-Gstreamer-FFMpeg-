using System.Collections.Generic;
using UnityEngine;

public class ChangeConnectedVertexColor : MonoBehaviour
{
    public Camera mainCamera;
    public Color colorToChange = Color.red;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (meshCollider != null && meshCollider.sharedMesh != null)
                {
                    Mesh mesh = meshCollider.sharedMesh;

                    // �浹�� �ﰢ���� index
                    int triangleIndex = hit.triangleIndex;

                    // ������ ��� vertex���� ������ ����
                    ChangeConnectedVertexColors(mesh, triangleIndex);
                }
            }
        }
    }

    void ChangeConnectedVertexColors(Mesh mesh, int initialTriangleIndex)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Color[] colors = mesh.colors;

        if (colors.Length == 0)
        {
            colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white; // �ʱ� ������ ������� ����
            }
        }

        // �ﰢ���� �̷�� vertex index��
        int vertexIndex1 = triangles[initialTriangleIndex * 3 + 0];
        int vertexIndex2 = triangles[initialTriangleIndex * 3 + 1];
        int vertexIndex3 = triangles[initialTriangleIndex * 3 + 2];

        // BFS�� ���� ť�� �湮�� vertex ����
        Queue<int> verticesToProcess = new Queue<int>();
        HashSet<int> visitedVertices = new HashSet<int>();

        // �ʱ� �ﰢ���� vertex���� ť�� �߰�
        verticesToProcess.Enqueue(vertexIndex1);
        verticesToProcess.Enqueue(vertexIndex2);
        verticesToProcess.Enqueue(vertexIndex3);

        // BFS�� ���� ������ ��� vertex Ž��
        while (verticesToProcess.Count > 0)
        {
            int currentVertex = verticesToProcess.Dequeue();

            // �̹� �湮�� vertex�� �ǳʶ�
            if (!visitedVertices.Add(currentVertex))
                continue;

            // ���� vertex�� ������ ����
            colors[currentVertex] = colorToChange;

            // ���� vertex�� ����� �ﰢ������ ã�� �� vertex���� ť�� �߰�
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (triangles[i] == currentVertex || triangles[i + 1] == currentVertex || triangles[i + 2] == currentVertex)
                {
                    verticesToProcess.Enqueue(triangles[i]);
                    verticesToProcess.Enqueue(triangles[i + 1]);
                    verticesToProcess.Enqueue(triangles[i + 2]);
                }
            }
        }

        // ����� colors �迭�� Mesh�� ����
        mesh.colors = colors;
    }
}
