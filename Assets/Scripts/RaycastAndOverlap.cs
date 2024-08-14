using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class RaycastAndOverlap : MonoBehaviour
{
    public float rayDistance = 100f; // Ray의 최대 거리
    public LayerMask layerMask;      // Raycast에 사용할 레이어 마스크
    public Material mat;
    public Transform FindTargetParent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
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

                        // b3dm 파일 경로에서 baseUrl을 추출
                        string fullB3dmPath = tileName + ".b3dm";  // 예: "http://localhost:3000/data/new/sinsung/Data/c10/f20301/d040.b3dm"
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

            // 루트 노드에서 시작하여 모든 .b3dm URI를 탐색합니다.
            FindAllB3dmUris(rootObject.root, url);
        }
    }

    void FindAllB3dmUris(TileNode node, string baseUrl)
    {
        // 현재 노드의 content.uri를 확인합니다.
        if (node.content != null && node.content.uri != null)
        {
            if (node.content.uri.EndsWith(".b3dm"))
            {
                string fullUri = baseUrl.Substring(0, baseUrl.LastIndexOf('/') + 1) + node.content.uri;

                // 이름이 'fullUri'와 정확히 일치하는 오브젝트를 찾습니다.
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

        // 하위 노드들에 대해 재귀적으로 탐색합니다.
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

            // 재귀적으로 자식의 자식들도 탐색
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

// tileset.json의 구조에 맞는 클래스 정의
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
