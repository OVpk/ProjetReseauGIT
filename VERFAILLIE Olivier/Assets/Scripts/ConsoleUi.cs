using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ConsoleUi : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private int maxLineCount = 10;

    private int lineCount = 0;
    private string myLog;

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        logString = type switch
        {
            LogType.Assert => "<color=white>" + logString + "</color>",
            LogType.Log => "<color=white>" + logString + "</color>",
            LogType.Warning => "<color=yellow>" + logString + "</color>",
            LogType.Exception => "<color=red>" + logString + "</color>",
            LogType.Error => "<color=red>" + logString + "</color>",
            _ => logString
        };

        myLog = myLog + "\n" + logString;
        lineCount++;

        if (lineCount > maxLineCount)
        {
            lineCount--;
            myLog = DeleteLines(myLog, 1);
        }

        text.text = myLog;
    }

    private string DeleteLines(string message, int linesToRemove)
    {
        return message.Split(Environment.NewLine.ToCharArray(), linesToRemove + 1).Skip(linesToRemove).FirstOrDefault();
    }
}
