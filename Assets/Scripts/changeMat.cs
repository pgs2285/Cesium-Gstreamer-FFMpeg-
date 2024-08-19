using UnityEngine;

public class UVRemap : MonoBehaviour
{
    public Mesh originalMesh; // ���� �޽�
    public Mesh newMesh; // Xatlas�� ���� ������ ���ο� �޽�
    public Texture2D originalTexture; // ���� �ؽ�ó

    void Start()
    {
        // ���ο� ���� ������Ʈ ����
        GameObject newObject = new GameObject("RemappedMesh");
        MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();

        // ���� �޽��� ���ο� �޽��� �����Ǿ� �ִ��� Ȯ��
        if (originalMesh == null || newMesh == null)
        {
            Debug.LogError("���� �޽� �Ǵ� ���ο� �޽��� �������� �ʾҽ��ϴ�.");
            return;
        }

        // ���ο� �޽� �ν��Ͻ� ����
        Mesh newMeshInstance = Instantiate(newMesh);

        // ���ο� �޽��� ���Ϳ� �Ҵ�
        meshFilter.mesh = newMeshInstance;

        // �ؽ�ó ������
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

        // ���ο� �ؽ�ó �ʱ�ȭ
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        // ���ο� �޽��� �� ���ؽ��� ���� ���� �޽��� ���� ���ؽ��� ã��
        for (int i = 0; i < newVertices.Length; i++)
        {
            int index = FindMatchingVertex(originalVertices, newVertices[i]);

            if (index != -1)
            {
                // ���� �޽��� UV ��ǥ�� ���� ���ø�
                Vector2 originalUV = originalUVs[index];
                Color sampledColor = originalTexture.GetPixelBilinear(originalUV.x, originalUV.y);

                // ���ο� UV ��ǥ�� ���� �Ҵ�
                Vector2 newUV = newUVs[i];
                int x = Mathf.FloorToInt(newUV.x * width);
                int y = Mathf.FloorToInt(newUV.y * height);

                // UV ���� �� ���� ����
                FillTexture(newTexture, x, y, sampledColor);
            }
        }

        newTexture.Apply();
        return newTexture;
    }

    void FillTexture(Texture2D texture, int x, int y, Color color)
    {
        // �߽� �ȼ��� ���� �Ҵ�
        texture.SetPixel(x, y, color);

        // �ֺ� �ȼ� ���� (�� �ε巴�� ����)
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
        return -1; // ��Ī�Ǵ� ���ؽ��� ã�� ���� ���
    }
}
