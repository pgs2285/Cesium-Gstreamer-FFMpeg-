using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;

public class GetRTSPVideo : MonoBehaviour
{
    public string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv001.stream";
    public RawImage targetRawImage;

    private Process ffmpegProcess;
    private Texture2D videoTexture;
    private byte[] frameData;
    private bool isStreaming;
    private int frameSize;

    private Queue<byte[]> frameQueue = new Queue<byte[]>();
    private object frameQueueLock = new object();

    void Start()
    {
        // Set up the texture and RawImage
        videoTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
        targetRawImage.texture = videoTexture;
        frameSize = 640 * 480 * 3;
        frameData = new byte[frameSize];

        StartCoroutine(CaptureVideo());
        StartCoroutine(UpdateTexture());
    }

    IEnumerator CaptureVideo()
    {
        // Start the FFmpeg process
        ffmpegProcess = new Process();
#if UNITY_STANDALONE_WIN
        ffmpegProcess.StartInfo.FileName = Application.dataPath + "/FFmpeg/ffmpeg.exe"; // Adjust the path to your FFmpeg binary
#endif
        ffmpegProcess.StartInfo.Arguments = $"-fflags +genpts -rtsp_transport tcp -i {rtspUrl} -f rawvideo -pix_fmt rgb24 -s 640x480 -r 30 -";
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true; // Redirect standard error to capture FFmpeg errors
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.Start();

        isStreaming = true;

        // Read error output in a separate thread to avoid blocking
        new Thread(() =>
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

        // Read the video frame data from FFmpeg's standard output in a separate thread
        new Thread(() =>
        {
            while (isStreaming)
            {
                try
                {
                    int bytesRead = 0;
                    while (bytesRead < frameSize)
                    {
                        int read = ffmpegProcess.StandardOutput.BaseStream.Read(frameData, bytesRead, frameSize - bytesRead);
                        if (read == 0)
                            break;
                        bytesRead += read;
                    }

                    if (bytesRead == frameSize)
                    {
                        lock (frameQueueLock)
                        {
                            frameQueue.Enqueue((byte[])frameData.Clone());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading from FFmpeg stream: {e.Message}");
                    isStreaming = false;
                }
            }
        }).Start();

        yield return null;
    }

    IEnumerator UpdateTexture()
    {
        while (isStreaming)
        {
            if (frameQueue.Count > 0)
            {
                byte[] frame = null;
                lock (frameQueueLock)
                {
                    frame = frameQueue.Dequeue();
                }

                if (frame != null)
                {
                    videoTexture.LoadRawTextureData(frame);
                    videoTexture.Apply();
                }
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
