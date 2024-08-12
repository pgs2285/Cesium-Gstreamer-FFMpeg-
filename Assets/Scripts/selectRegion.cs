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

                    // 충돌한 삼각형의 index
                    int triangleIndex = hit.triangleIndex;

                    // 연관된 모든 vertex들의 색상을 변경
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
                colors[i] = Color.white; // 초기 색상은 흰색으로 설정
            }
        }

        // 삼각형을 이루는 vertex index들
        int vertexIndex1 = triangles[initialTriangleIndex * 3 + 0];
        int vertexIndex2 = triangles[initialTriangleIndex * 3 + 1];
        int vertexIndex3 = triangles[initialTriangleIndex * 3 + 2];

        // BFS를 위한 큐와 방문한 vertex 추적
        Queue<int> verticesToProcess = new Queue<int>();
        HashSet<int> visitedVertices = new HashSet<int>();

        // 초기 삼각형의 vertex들을 큐에 추가
        verticesToProcess.Enqueue(vertexIndex1);
        verticesToProcess.Enqueue(vertexIndex2);
        verticesToProcess.Enqueue(vertexIndex3);

        // BFS를 통해 연관된 모든 vertex 탐색
        while (verticesToProcess.Count > 0)
        {
            int currentVertex = verticesToProcess.Dequeue();

            // 이미 방문한 vertex는 건너뜀
            if (!visitedVertices.Add(currentVertex))
                continue;

            // 현재 vertex의 색상을 변경
            colors[currentVertex] = colorToChange;

            // 현재 vertex와 연결된 삼각형들을 찾아 그 vertex들을 큐에 추가
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

        // 변경된 colors 배열을 Mesh에 적용
        mesh.colors = colors;
    }
}
