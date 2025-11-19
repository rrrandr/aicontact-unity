using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class RegisterVirtualCamera : MonoBehaviour
{
    private void Start()
    {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        foreach (WebCamDevice device in WebCamTexture.devices.Where(d => d.name == "UnityCam"))
        {
            UnityEngine.Debug.Log(device.name + " virtual camera already exists");
            return;
        }

        string directoryPath = Application.dataPath + @"\x64\Register.bat";
        directoryPath = directoryPath.Replace(@"/", @"\");
        UnityEngine.Debug.Log("Path: " + directoryPath);

        if (File.Exists(directoryPath))
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{directoryPath}\"",
                    Verb = "runas", // Request elevated privileges
                    UseShellExecute = true, // Needed for Verb
                    CreateNoWindow = false  // Optional: show the window
                };

                Process process = Process.Start(processInfo);
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log("Batch file executed successfully.");
                }
                else
                {
                    UnityEngine.Debug.LogError($"Batch file execution failed.");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Error executing the batch file: " + ex.Message);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("This directory path doesn't exist: " + directoryPath);
        }

#endif
    }
}
