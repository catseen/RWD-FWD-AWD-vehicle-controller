using UnityEngine;

public static class Logger
{
    public static bool enableLogs = true; 

    public static void Log(string message)
    {
        if (enableLogs)
        {
            Debug.Log(message);
        }
    }
}
