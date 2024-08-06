using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System;
using Debug = UnityEngine.Debug;

public class GetRTSPVideo : MonoBehaviour
{
    public string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv001.stream";
    public RawImage targetRawImage;

    private Process ffmpegProcess;
    private Texture2D videoTexture;
    private bool isStreaming;

    void Start()
    {
        // Set up the texture and RawImage
        videoTexture = new Texture2D(320, 240, TextureFormat.RGB24, false);
        targetRawImage.texture = videoTexture;
        StartCoroutine(CaptureVideo());
    }

    IEnumerator CaptureVideo()
    {
        // Start the FFmpeg process
        ffmpegProcess = new Process();
#if UNITY_STANDALONE_WIN
        ffmpegProcess.StartInfo.FileName = Application.dataPath + "/FFmpeg/ffmpeg.exe"; // Adjust the path to your FFmpeg binary
#endif
        ffmpegProcess.StartInfo.Arguments = $"-rtsp_transport tcp -re -i {rtspUrl} -f rawvideo -pix_fmt rgb24 -s 320x240 -r 6 -vsync 1 -";
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true; // Redirect standard error to capture FFmpeg errors
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.Start();

        isStreaming = true;

        // Read error output in a separate thread to avoid blocking
        new System.Threading.Thread(() =>
        {
            using (StreamReader reader = ffmpegProcess.StandardError)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Debug.Log(line); // Log FFmpeg errors
                }
            }
        }).Start();

        int frameSize = 320 * 240 * 3; // 3 bytes per pixel (RGB)
        byte[] frameData = new byte[frameSize];
        int offset = 0;

        while (isStreaming)
        {
            try
            {
                // Read the video frame data from FFmpeg's standard output
                int bytesRead = ffmpegProcess.StandardOutput.BaseStream.Read(frameData, offset, frameSize - offset);
                offset += bytesRead;

                if (offset == frameSize)
                {
                    // Load the image data into the texture
                    videoTexture.LoadRawTextureData(frameData);
                    videoTexture.Apply();
                    offset = 0; // Reset offset for next frame
                }
                else if (bytesRead == 0)
                {
                    Debug.LogWarning("No more data available from FFmpeg stream.");
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading from FFmpeg stream: {e.Message}");
                isStreaming = false;
            }

            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        // Stop the FFmpeg process
        isStreaming = false;

        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            ffmpegProcess.Kill();
            ffmpegProcess.Dispose();
        }
    }
}
