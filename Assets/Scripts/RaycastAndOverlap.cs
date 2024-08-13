using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class RaycastAndB3dmFinder : MonoBehaviour
{
    public float rayDistance = 100f; // Ray의 최대 거리
    public LayerMask layerMask;      // Raycast에 사용할 레이어 마스크
    public Material mat;
    private string baseUrl = "http://localhost:3000/data/new/sinsung/Data/c10/f20301/";
    public GameObject FindTargetParent;

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

                        string tilesetUrl = baseUrl + "tileset.json";

                        StartCoroutine(LoadTilesetJson(tilesetUrl));
                    }
                }
                else
                {
                    Debug.Log("No objects hit by the overlap box.");
                }

                // 시각적으로 확인하기 위해 충돌 지점에 큐브를 생성 (디버그 용도)
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cube.transform.position = hitPosition;
                //cube.transform.localScale = new Vector3(2f, 2f, 2f);
                //Destroy(cube, 5f); // 5초 후 큐브 제거
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

            // 루트 노드에서 시작하여 모든 .b3dm
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
                Debug.Log("Found b3dm URI: " + fullUri);

                // URI에서 파일 이름 추출 
                string b3dmFileName = Path.GetFileNameWithoutExtension(node.content.uri);

                // Material 변경
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

        // 하위 노드들에 대해 재귀적으로 탐색합니다.
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
