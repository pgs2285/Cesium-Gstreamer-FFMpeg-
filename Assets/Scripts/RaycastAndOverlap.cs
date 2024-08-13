using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class RaycastAndB3dmFinder : MonoBehaviour
{
    public float rayDistance = 100f; // Ray�� �ִ� �Ÿ�
    public LayerMask layerMask;      // Raycast�� ����� ���̾� ����ũ
    public Material mat;
    private string baseUrl = "http://localhost:3000/data/new/sinsung/Data/c10/f20301/";
    public GameObject FindTargetParent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ��
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
            {
                Vector3 hitPosition = hit.point;
                Collider[] colliders = Physics.OverlapBox(hitPosition, new Vector3(1f, 1f, 1f), Quaternion.identity, layerMask);

                if (colliders.Length > 0)
                {
                    Debug.Log("Objects hit by the overlap box:");

                    foreach (Collider collider in colliders)
                    {
                        string tileName = collider.gameObject.transform.parent.name;
                        Debug.Log("Tile Name: " + tileName);

                        string tilesetUrl = baseUrl + "tileset.json";

                        StartCoroutine(LoadTilesetJson(tilesetUrl));
                    }
                }
                else
                {
                    Debug.Log("No objects hit by the overlap box.");
                }

                // �ð������� Ȯ���ϱ� ���� �浹 ������ ť�긦 ���� (����� �뵵)
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cube.transform.position = hitPosition;
                //cube.transform.localScale = new Vector3(2f, 2f, 2f);
                //Destroy(cube, 5f); // 5�� �� ť�� ����
            }
        }
    }

    IEnumerator LoadTilesetJson(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string jsonData = www.downloadHandler.text;
   
            TilesetRoot rootObject = JsonUtility.FromJson<TilesetRoot>(jsonData);

            // ��Ʈ ��忡�� �����Ͽ� ��� .b3dm
            FindAllB3dmUris(rootObject.root, url);
        }
    }

    void FindAllB3dmUris(TileNode node, string baseUrl)
    {
        // ���� ����� content.uri�� Ȯ���մϴ�.
        if (node.content != null && node.content.uri != null)
        {
            if (node.content.uri.EndsWith(".b3dm"))
            {
                string fullUri = baseUrl.Substring(0, baseUrl.LastIndexOf('/') + 1) + node.content.uri;
                Debug.Log("Found b3dm URI: " + fullUri);

                // URI���� ���� �̸� ���� 
                string b3dmFileName = Path.GetFileNameWithoutExtension(node.content.uri);

                // Material ����
                GameObject foundObject = GameObject.Find(fullUri);
                if (foundObject != null)
                {
                    ChangeObjectColor(foundObject, Color.red);
                }
                else
                {
                    Debug.LogWarning("GameObject not found for: " + b3dmFileName);
                }
            }
        }

        // ���� ���鿡 ���� ��������� Ž���մϴ�.
        if (node.children != null && node.children.Length > 0)
        {
            foreach (var child in node.children)
            {
                FindAllB3dmUris(child, baseUrl);
            }
        }
    }

    void ChangeObjectColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = mat;
            Debug.Log("Changed color of: " + obj.name);
        }
        else
        {
            Debug.LogWarning("Renderer not found on: " + obj.name);
        }
    }
}

// tileset.json�� ������ �´� Ŭ���� ����
[Serializable]
public class TilesetRoot
{
    public TileNode root;
}

[Serializable]
public class TileNode
{
    public Content content;
    public TileNode[] children;
}

[Serializable]
public class Content
{
    public string uri;
}
