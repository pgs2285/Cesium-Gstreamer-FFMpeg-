using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

using Debug = UnityEngine.Debug;
public class Final : MonoBehaviour
{
    private static int vertexOffset = 0;
    private static int normalOffset = 0;
    private static int uvOffset = 0;
    public GameObject[] ObjectList;

    private static string targetFolder = Path.Combine(Application.dataPath, "XAtlas");

    private static string floatToStr(float number)
    {
        return string.Format("{0:0.######}", number);
    }

    private static string getRandomStr()
    {
        string s = Path.GetRandomFileName() + System.DateTime.Now.Millisecond.ToString();
        s = s.Replace(".", "");
        return s;
    }

    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        string groupName = mats[0].name;

        sb.Append("g ").Append(groupName).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 wv = mf.transform.TransformPoint(lv);

            sb.Append(string.Format(
                "v {0} {1} {2}\n",
                floatToStr(-wv.x),
                floatToStr(wv.y),
                floatToStr(wv.z)
            ));
        }

        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.TransformDirection(lv);

            sb.Append(string.Format(
                "vn {0} {1} {2}\n",
                floatToStr(-wv.x),
                floatToStr(wv.y),
                floatToStr(wv.z)
            ));
        }

        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format(
                "vt {0} {1}\n",
                floatToStr(v.x),
                floatToStr(v.y)
            ));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            try
            {
                ObjMaterial objMaterial = new ObjMaterial { name = mats[material].name };

                if (mats[material].mainTexture)
                {
                    string texturePath = AssetDatabase.GetAssetPath(mats[material].mainTexture);
                    string textureFileName = Path.GetFileName(texturePath);
                    string targetTexturePath = Path.Combine(targetFolder, textureFileName);
                    File.Copy(texturePath, targetTexturePath, true);
                    objMaterial.textureName = textureFileName;
                }
                else
                {
                    objMaterial.textureName = null;
                }

                materialList.Add(objMaterial.name, objMaterial);
            }
            catch (System.ArgumentException)
            {
                // �̹� �߰��� ������ ���� ��� ���� ����
            }

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(
                    string.Format(
                        "f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                        triangles[i + 0] + 1 + vertexOffset,
                        triangles[i + 1] + 1 + normalOffset,
                        triangles[i + 2] + 1 + uvOffset
                    )
                );
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }

    private static void MeshToFile(GameObject go, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        string mtlFileName = filename + ".mtl";
        string mtlRelativePath = "./" + mtlFileName;

        using (StreamWriter sw = new StreamWriter(Path.Combine(folder, filename + ".obj")))
        {
            sw.Write("mtllib " + mtlRelativePath + "\n");

            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < mfs.Length; i++)
            {
                sw.Write(MeshToString(mfs[i], materialList));
            }
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
    {
        Clear();
        return new Dictionary<string, ObjMaterial>();
    }

    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
    {
        using (StreamWriter sw = new StreamWriter(Path.Combine(folder, filename + ".mtl")))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\nnewmtl " + kvp.Key + "\n");

                if (!string.IsNullOrEmpty(kvp.Value.textureName))
                {
                    string relativeTexturePath = kvp.Value.textureName;
                    sw.Write("map_Kd " + relativeTexturePath + "\n");
                }
                else
                {
                    sw.Write("map_Kd none\n");
                }
            }
        }
    }

    private static void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }

    private static bool CreateTargetFolder()
    {
        try
        {
            Directory.CreateDirectory(targetFolder);
        }
        catch
        {
            return false;
        }

        return true;
    }
    private static void MeshesToFile(GameObject[] gos, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        string mtlFileName = filename + ".mtl";
        string mtlRelativePath = "./" + mtlFileName;  // ����η� ����

        // �������� �� ���� �ʱ�ȭ�մϴ�.
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;

        using (StreamWriter sw = new StreamWriter(Path.Combine(folder, filename + ".obj")))
        {
            sw.Write("mtllib " + mtlRelativePath + "\n");

            // �� GameObject�� ���������� ó��
            for (int i = 0; i < gos.Length; i++)
            {
                MeshFilter[] mfs = gos[i].GetComponentsInChildren<MeshFilter>();
                for (int j = 0; j < mfs.Length; j++)
                {
                    // �������� �ʱ�ȭ���� �ʰ�, �����ǵ��� �մϴ�.
                    sw.Write(MeshToString(mfs[j], materialList));
                }
            }
        }
        GetShaderTest getShaderTest = new GetShaderTest();
        getShaderTest.SaveMaterialTexture(filename);
        MaterialsToFile(materialList, folder, filename);
    }

    public void ExportSelectionToOBJ()
    {
        if (!CreateTargetFolder())
        {
            Debug.LogError("Ÿ�� ���� ���� ����: " + targetFolder);
            return;
        }

        GameObject[] selectedObjects = GetSelectedObjects();

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("���õ� ������Ʈ�� �����ϴ�.");
            return;
        }

        string filename = "MERGED-" + getRandomStr();
        MeshesToFile(selectedObjects, targetFolder, filename);

        RunXAtlasAndReimport(filename);
    }

    private GameObject[] GetSelectedObjects()
    {
        return ObjectList;
    }

    private void RunXAtlasAndReimport(string filename)
    {
        string objPath = Path.Combine(targetFolder, filename + ".obj");
        string exePath = Path.Combine(Application.dataPath, "XAtlas/example_repack.exe");

        ProcessStartInfo processInfo = new ProcessStartInfo();
        processInfo.FileName = exePath;
        processInfo.Arguments = ".\\" + filename + ".obj";
        processInfo.WorkingDirectory = targetFolder;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;

        Debug.Log("XAtlas ���� ��...");
        Debug.Log("��ɾ�: " + exePath + " " + processInfo.Arguments);

        Process process = Process.Start(processInfo);

        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            Debug.Log(result);
        }

        string outputObjPath = objPath;
        //LoadAndInstantiateObject(outputObjPath);
    }

    private void LoadAndInstantiateObject(string objPath)
    {
        OBJLoader loader = new OBJLoader();
        GameObject importedObject = loader.Load(objPath);

        if (importedObject != null)
        {
            Debug.Log("����ŷ�� ������Ʈ �ε� ����.");
            Instantiate(importedObject, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("����ŷ�� ������Ʈ �ε� ����.");
        }
    }

    public void CombineMeshes()
    {
        GameObject[] selectedObjects = GetSelectedObjects();

        List<CombineInstance> combine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        int vertexOffset = 0;

        foreach (GameObject go in selectedObjects)
        {
            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in meshFilters)
            {
                Mesh mesh = mf.sharedMesh;
                Renderer renderer = mf.GetComponent<Renderer>();

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = mesh;
                    ci.subMeshIndex = i;
                    ci.transform = mf.transform.localToWorldMatrix;
                    combine.Add(ci);

                    materials.Add(renderer.sharedMaterials[i]);
                }

                vertexOffset += mesh.vertexCount;
            }
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine.ToArray(), false, true);  // false�� �����Ͽ� ���� ����޽ø� ����

        // ���յ� �޽ø� ���ο� GameObject�� �Ҵ�
        GameObject combinedObject = new GameObject("CombinedMesh");
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;

        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = materials.ToArray();  // ���յ� ��Ƽ������ �Ҵ�

        // ���յ� ������Ʈ�� ExportSelectionToOBJ�� ��������
        ObjectList = new GameObject[] { combinedObject };
        ExportSelectionToOBJ();
    }


    private void Start()
    {
        CombineMeshes();
    }
}

struct ObjMaterial
{
    public string name;
    public string textureName;
}
