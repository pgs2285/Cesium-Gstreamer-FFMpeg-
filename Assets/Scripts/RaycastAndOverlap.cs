using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class RaycastAndOverlap : MonoBehaviour
{
    public float rayDistance = 100f; // Ray�� �ִ� �Ÿ�
    public LayerMask layerMask;      // Raycast�� ����� ���̾� ����ũ
    public Material mat;
    public Transform FindTargetParent;

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

                        // b3dm ���� ��ο��� baseUrl�� ����
                        string fullB3dmPath = tileName + ".b3dm";  // ��: "http://localhost:3000/data/new/sinsung/Data/c10/f20301/d040.b3dm"
                        string baseUrl = Path.GetDirectoryName(fullB3dmPath).Replace("\\", "/") + "/";

                        string tilesetUrl = baseUrl + "tileset.json";
                        if (tilesetUrl.StartsWith("http:/"))
                            tilesetUrl = tilesetUrl.Replace("http:/", "http://");

                        StartCoroutine(LoadTilesetJson(tilesetUrl));
                    }
                }
                else
                {
                    Debug.Log("No objects hit by the overlap box.");
                }
            }
        }
    }

    IEnumerator LoadTilesetJson(string url)
    {
        Debug.Log(url);
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

            // ��Ʈ ��忡�� �����Ͽ� ��� .b3dm URI�� Ž���մϴ�.
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

                // �̸��� 'fullUri'�� ��Ȯ�� ��ġ�ϴ� ������Ʈ�� ã���ϴ�.
                GameObject foundObject = FindChildByExactName(FindTargetParent, fullUri);
                Debug.Log("Searching for: " + fullUri);

                if (foundObject != null)
                {
                    ChangeObjectColor(foundObject, Color.red);
                    Debug.Log("Found GameObject: " + foundObject.name);
                }
                else
                {
                    Debug.LogWarning("GameObject not found for URI: " + fullUri);
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

    GameObject FindChildByExactName(Transform parent, string exactName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == exactName)
            {
                return child.gameObject;
            }

            // ��������� �ڽ��� �ڽĵ鵵 Ž��
            GameObject found = FindChildByExactName(child, exactName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
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
