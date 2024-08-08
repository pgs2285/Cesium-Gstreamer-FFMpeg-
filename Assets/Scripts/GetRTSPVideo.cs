using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
<<<<<<< HEAD
using System.Threading;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
=======
using System;
using Debug = UnityEngine.Debug;
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762

public class GetRTSPVideo : MonoBehaviour
{
    public string rtspUrl = "rtsp://210.99.70.120:1935/live/cctv001.stream";
    public RawImage targetRawImage;

    private Process ffmpegProcess;
    private Texture2D videoTexture;
<<<<<<< HEAD
    private byte[] frameData;
    private bool isStreaming;
    private int frameSize;

    private Queue<byte[]> frameQueue = new Queue<byte[]>();
    private object frameQueueLock = new object();
=======
    private bool isStreaming;
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762

    void Start()
    {
        // Set up the texture and RawImage
<<<<<<< HEAD
        videoTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
        targetRawImage.texture = videoTexture;
        frameSize = 640 * 480 * 3;
        frameData = new byte[frameSize];

        StartCoroutine(CaptureVideo());
        StartCoroutine(UpdateTexture());
=======
        videoTexture = new Texture2D(320, 240, TextureFormat.RGB24, false);
        targetRawImage.texture = videoTexture;
        StartCoroutine(CaptureVideo());
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762
    }

    IEnumerator CaptureVideo()
    {
        // Start the FFmpeg process
        ffmpegProcess = new Process();
#if UNITY_STANDALONE_WIN
        ffmpegProcess.StartInfo.FileName = Application.dataPath + "/FFmpeg/ffmpeg.exe"; // Adjust the path to your FFmpeg binary
#endif
<<<<<<< HEAD
        ffmpegProcess.StartInfo.Arguments = $" -flags low_delay -strict experimental -rtsp_transport tcp -i {rtspUrl} -f rawvideo -pix_fmt rgb24 -s 640x480 -r 30 -";
=======
        ffmpegProcess.StartInfo.Arguments = $"-rtsp_transport tcp -re -i {rtspUrl} -f rawvideo -pix_fmt rgb24 -s 320x240 -r 6 -vsync 1 -";
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true; // Redirect standard error to capture FFmpeg errors
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.Start();

        isStreaming = true;

        // Read error output in a separate thread to avoid blocking
<<<<<<< HEAD
        new Thread(() =>
=======
        new System.Threading.Thread(() =>
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762
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

<<<<<<< HEAD
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
=======
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
>>>>>>> 9971ba4bfb3b64a981b9c73d952c63b595e43762
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
