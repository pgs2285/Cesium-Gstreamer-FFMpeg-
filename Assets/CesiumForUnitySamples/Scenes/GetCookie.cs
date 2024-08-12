using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class GetCookie : MonoBehaviour
{
    private string loginUrl = "https://demo3d.sistech3d.com/api/login";
    private string dataUrl = "http://localhost:3000/data/new/sinsung/tileset.json";

    //      https://demo3d.sistech3d.com/data/new/tanbang/Data/e1214/tileset.json
    private string token;
    public CesiumForUnity.Cesium3DTileset cesiumTileset;

    void Start()
    {
        StartCoroutine(SendGuestLoginRequest());
    }

    IEnumerator SendGuestLoginRequest()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.PostWwwForm(loginUrl, "");
            www.SetRequestHeader("Accept", "application/json, text/plain, */*");
            www.SetRequestHeader("Origin", "https://demo3d.sistech3d.com");
            www.SetRequestHeader("Referer", "https://demo3d.sistech3d.com/");
            www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // 응답에서 토큰 추출
                Dictionary<string, string> responseHeaders = www.GetResponseHeaders();
                foreach (KeyValuePair<string, string> header in responseHeaders)
                {
                    if (header.Key.Equals("Set-Cookie"))
                    {
                        token = header.Value;
                        UpdateProxyServerWithToken(token);
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(token))
                {
                    LoadTileset();
                }
                else
                {
                    Debug.LogError("Token was not found in response headers.");
                }
            }
            yield return new WaitForSeconds(3000f);
        }

    }

    void UpdateProxyServerWithToken(string token)
    {
        string proxyUrl = "http://localhost:3000/set-token?token=" + token;
        StartCoroutine(SendTokenToProxy(proxyUrl));
    }

    IEnumerator SendTokenToProxy(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Token updated on proxy server: " + www.downloadHandler.text);
            LoadTileset();
        }
    }

    void LoadTileset()
    {
        if (cesiumTileset != null)
        {
            cesiumTileset.url = dataUrl;
            cesiumTileset.RecreateTileset();
        }
        else
        {
            Debug.LogError("Cesium3DTileset component is not assigned.");
        }
    }
}
