using UnityEngine;

public class UVRemap : MonoBehaviour
{
    public Mesh originalMesh; // 원본 메쉬
    public Mesh newMesh; // Xatlas를 통해 생성된 새로운 메쉬
    public Texture2D originalTexture; // 원본 텍스처

    void Start()
    {
        // 새로운 게임 오브젝트 생성
        GameObject newObject = new GameObject("RemappedMesh");
        MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();

        // 원본 메쉬와 새로운 메쉬가 설정되어 있는지 확인
        if (originalMesh == null || newMesh == null)
        {
            Debug.LogError("원본 메쉬 또는 새로운 메쉬가 설정되지 않았습니다.");
            return;
        }

        // 새로운 메쉬 인스턴스 생성
        Mesh newMeshInstance = Instantiate(newMesh);

        // 새로운 메쉬를 필터에 할당
        meshFilter.mesh = newMeshInstance;

        // 텍스처 리맵핑
        Texture2D newTexture = RemapTexture(originalMesh, newMeshInstance, originalTexture);
        meshRenderer.material.mainTexture = newTexture;
    }

    Texture2D RemapTexture(Mesh originalMesh, Mesh newMesh, Texture2D originalTexture)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        Vector2[] originalUVs = originalMesh.uv;
        Vector3[] newVertices = newMesh.vertices;
        Vector2[] newUVs = newMesh.uv;

        int width = originalTexture.width;
        int height = originalTexture.height;
        Texture2D newTexture = new Texture2D(width, height);

        // 새로운 텍스처 초기화
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        // 새로운 메쉬의 각 버텍스에 대해 원본 메쉬의 대응 버텍스를 찾음
        for (int i = 0; i < newVertices.Length; i++)
        {
            int index = FindMatchingVertex(originalVertices, newVertices[i]);

            if (index != -1)
            {
                // 원본 메쉬의 UV 좌표로 색상 샘플링
                Vector2 originalUV = originalUVs[index];
                Color sampledColor = originalTexture.GetPixelBilinear(originalUV.x, originalUV.y);

                // 새로운 UV 좌표에 색상 할당
                Vector2 newUV = newUVs[i];
                int x = Mathf.FloorToInt(newUV.x * width);
                int y = Mathf.FloorToInt(newUV.y * height);

                // UV 보간 및 색상 설정
                FillTexture(newTexture, x, y, sampledColor);
            }
        }

        newTexture.Apply();
        return newTexture;
    }

    void FillTexture(Texture2D texture, int x, int y, Color color)
    {
        // 중심 픽셀에 색상 할당
        texture.SetPixel(x, y, color);

        // 주변 픽셀 보간 (더 부드럽게 매핑)
        texture.SetPixel(x + 1, y, color);
        texture.SetPixel(x - 1, y, color);
        texture.SetPixel(x, y + 1, color);
        texture.SetPixel(x, y - 1, color);
    }

    int FindMatchingVertex(Vector3[] originalVertices, Vector3 targetVertex)
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (Vector3.Distance(originalVertices[i], targetVertex) < 0.0001f)
            {
                return i;
            }
        }
        return -1; // 매칭되는 버텍스를 찾지 못한 경우
    }
}
