using UnityEngine;

public class GetShaderTest : MonoBehaviour
{
    public Material cesiumMaterial; // ���� ���̴��� ����� ���͸���
    public Texture2D baseColorTexture;

    public void SaveMaterialTexture(string filename)
    {
   
        // Cesium ���̴����� baseColorTexture�� ��������
        cesiumMaterial = GetComponent<MeshRenderer>().material;
        baseColorTexture = cesiumMaterial.GetTexture("_baseColorTexture") as Texture2D;

        if (baseColorTexture != null)
        {
            // RenderTexture�� ����Ͽ� �ؽ�ó�� �б� ������ Texture2D�� ����
            Texture2D readableTexture = GetReadableTexture(baseColorTexture);

            // �ؽ�ó�� ���Ϸ� ����
            SaveTextureToFile(readableTexture, $"XAtlas/{filename}.jpg");
        }
        else
        {
            Debug.LogError("baseColorTexture�� ã�� �� �����ϴ�.");
        }
    }
    

    Texture2D GetReadableTexture(Texture texture)
    {
        // RenderTexture ����
        RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0);
        Graphics.Blit(texture, renderTexture);

        // RenderTexture�� �б� ������ Texture2D�� ����
        Texture2D readableTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTexture.Apply();

        // RenderTexture Ȱ��ȭ ���� �� �޸� ����
        RenderTexture.active = null;
        renderTexture.Release();

        return readableTexture;
    }

    void SaveTextureToFile(Texture2D texture, string path)
    {
        // �ؽ�ó �����͸� PNG�� ��ȯ
        byte[] bytes = texture.EncodeToJPG();

        // ���Ϸ� ����
        System.IO.File.WriteAllBytes(path, bytes);

        Debug.Log("Texture saved to: " + path);
    }
}
