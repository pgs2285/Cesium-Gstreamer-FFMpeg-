using System.Diagnostics;
using System.IO;
using UnityEngine;

public class StartProxyServer : MonoBehaviour
{
    private Process nodeProcess;

    void Start()
    {
        StartNodeServer();
    }

    void StartNodeServer()
    {
        string nodePath = "node"; // Node.js�� ��ΰ� PATH�� �߰��Ǿ� ���� ���
        string proxyScriptPath = Path.Combine(Application.dataPath, "proxy.js");

        ProcessStartInfo startInfo = new ProcessStartInfo(nodePath)
        {
            Arguments = proxyScriptPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        nodeProcess = new Process
        {
            StartInfo = startInfo
        };

        nodeProcess.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
        nodeProcess.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);

        nodeProcess.Start();
        nodeProcess.BeginOutputReadLine();
        nodeProcess.BeginErrorReadLine();
    }

    void OnApplicationQuit()
    {
        if (nodeProcess != null && !nodeProcess.HasExited)
        {
            nodeProcess.Kill();
        }
    }
}
