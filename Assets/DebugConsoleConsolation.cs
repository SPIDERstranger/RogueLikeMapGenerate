using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugConsoleConsolation : MonoBehaviour {
    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    public bool shakeToOpen = true;

    public float shakeAcceleration = 3f;

    public bool restrictLogCount = false;

    public int maxLogs = 1000;

    readonly List<Log> logs = new List<Log>();

    Vector2 scrollPosition;
    bool visible ;

    bool collapse;

    static readonly Dictionary<LogType,Color> logTypeColors = new Dictionary<LogType, Color>
    {
        {LogType.Assert,Color.white},
        {LogType.Error,Color.red},
        {LogType.Exception,Color.red},
        {LogType.Log,Color.white},
        {LogType.Warning,Color.yellow},
    };
    const string windowTitle = "Console";
    const int margin = 50;
    static readonly GUIContent clearLabel = new GUIContent("Clear","Clear the contents of the console.");
    static readonly GUIContent collapseLabel = new GUIContent("Collapse","Hide repeated message");
    
    readonly Rect titleBarRect = new Rect(0,0,1000,20);

    Rect windowRect = new Rect(margin,margin,Screen.width-(margin*2),Screen.height - (margin*2));

    private void OnEnable() {
        Application.logMessageReceived+=HandleLog;
    }
    private void OnDisable()
    {
        Application.logMessageReceived-=HandleLog;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            visible = !visible;
        }
        if(shakeToOpen&&Input.acceleration.sqrMagnitude>shakeAcceleration)
        {
            visible  = true;
        }
    }
    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,margin,margin),"console"))
        {
            visible = !visible;
        }
        if(!visible)
        {
            return;
        }
        windowRect = GUILayout.Window(123123,windowRect,DrawConsolWindow,windowTitle);
    }  
    private void DrawConsolWindow(int windowId)
    {
        DrawLogsList();
        DrawToolbar();

        GUI.DragWindow(titleBarRect);
    }
    private void  DrawLogsList(){
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for(int i = 0;i<logs.Count;i++)
        {
            var log = logs[i];
            if(collapse && i>0)
            {
                var previousMessage = logs[i-1].message;
                if(log.message == previousMessage)
                {
                    continue;
                }
            }
            GUI.contentColor = logTypeColors[log.type];
            GUILayout.Label(log.message);
        }

        GUILayout.EndScrollView();

        GUI.contentColor = Color.white;
    }
    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button(clearLabel))
        {
            logs.Clear();
        }

        collapse = GUILayout.Toggle(collapse,collapseLabel,GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    private void HandleLog(string message,string stackTrace,LogType type)
    {
        logs.Add(new Log{
            message = message,
            stackTrace = stackTrace,
            type = type,
        });
        TrimExcessLogs();
    }

    private void TrimExcessLogs()
    {
        if(!restrictLogCount)
        {
            return ;
        }
        var amountToRemove = Mathf.Max(logs.Count - maxLogs,0);
        if(amountToRemove == 0)
        {
            return ;
        }
        logs.RemoveRange(0,amountToRemove);
    }
}