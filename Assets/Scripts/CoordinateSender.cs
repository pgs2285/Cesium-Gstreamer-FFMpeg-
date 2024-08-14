using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class CoordinateSender : MonoBehaviour
{
    public string serverIP = "129.254.193.41";
    public int serverPort = 65432;

    // 위경도를 입력받아 서버로 전송하고 응답을 받는 메서드
    public void SendCoordinates(float latitude, float longitude)
    {
        try
        {
            using (TcpClient client = new TcpClient(serverIP, serverPort))
            {
                NetworkStream stream = client.GetStream();

                // 위경도를 서버에 보낼 데이터로 포맷팅
                string message = $"{latitude},{longitude}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                // 서버로 데이터 전송
                stream.Write(data, 0, data.Length);
                Debug.Log($"Sent: {message}");

                // 서버로부터 응답 수신
                data = new byte[1024];
                int bytes = stream.Read(data, 0, data.Length);
                string responseData = Encoding.UTF8.GetString(data, 0, bytes);
                Debug.Log($"Received: {responseData}");

                // 서버 응답 처리
                HandleServerResponse(responseData);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"SocketException: {e}");
        }
    }

    // 서버의 응답을 처리하는 메서드
    private void HandleServerResponse(string response)
    {
        if (response == "영역내에 해당하는 좌표가 없습니다.")
        {
            Debug.Log("해당 좌표를 포함하는 폴리곤을 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log($"폴리곤 정보: {response}");
            // 여기서 받은 폴리곤 정보를 활용할 수 있습니다.
        }
    }

    // 유니티에서 테스트하기 위한 예제
    void Start()
    {
        // 예제 좌표 (위도, 경도)
        float latitude = 36.351;
        float longitude = 127.385;

        SendCoordinates(latitude, longitude);
    }
}
