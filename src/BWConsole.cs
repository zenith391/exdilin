using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003A2 RID: 930
public class BWConsole : MonoBehaviour
{
	// Token: 0x06002888 RID: 10376 RVA: 0x0012A6B5 File Offset: 0x00128AB5
	public void OnEnable()
	{
		Application.RegisterLogCallback(HandleLog);
	}

	// Token: 0x06002889 RID: 10377 RVA: 0x0012A6C8 File Offset: 0x00128AC8
	public void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	// Token: 0x0600288A RID: 10378 RVA: 0x0012A6D0 File Offset: 0x00128AD0
	public void Start()
	{
		base.enabled = (BWEnvConfig.Flags.ContainsKey("DEBUG_CONSOLE") && BWEnvConfig.Flags["DEBUG_CONSOLE"]);
	}

    // Token: 0x0600288B RID: 10379 RVA: 0x0012A701 File Offset: 0x00128B01
    public void Update()
	{
		if (Input.GetKeyDown(this.toggleKey))
		{
            this._showConsole = !this._showConsole;
		}
	}

    // Token: 0x0600288C RID: 10380 RVA: 0x0012A724 File Offset: 0x00128B24
    public void OnGUI()
	{

		if (this._showConsole)
		{
			GUI.backgroundColor = Color.cyan;
			this._windowRect = GUILayout.Window(123456, this._windowRect, new GUI.WindowFunction(this.ConsoleWindow), "Blocksworld Console (press " + this.toggleKey + " to hide)", new GUILayoutOption[0]);
		}
	}

    public void CommandParse(string cmd)
    {
        string[] command = cmd.Split(' ');
        string name = command[0];
        if (name == "reload")
        {
            if (command.Length < 2)
            {
                BWLog.Error("[Console] 'reload' takes atleast 1 argument.");
            }
            string kind = command[1];
            if (kind == "worlds")
            {
                BWUserWorldsDataManager.Instance.LoadWorlds();
            }
            else if (kind == "world")
            {
                BWLocalWorld world = BWUserWorldsDataManager.Instance.GetWorldWithLocalWorldID(WorldSession.current.worldId);
                world.OverwriteSource(Util.ObfuscateSourceForUser("{}", BWUser.currentUser.userID), false);
                BWUserWorldsDataManager.Instance.LoadSourceForLocalWorld(world, null);
            }
            else
            {
                BWLog.Error("[Console] No such reload kind: " + kind);
            }
        }
        if (name == "save")
        {
            if (command.Length < 2)
            {
                BWLog.Error("[Console] 'save' takes atleast 1 argument.");
            }
            string kind = command[1];
            if (kind == "world")
            {
                BWStandalone.Instance.SaveCurrentWorldSession(null);
            }
            else
            {
                BWLog.Error("[Console] No such reload kind: " + kind);
            }
        }
		if (name == "export") {
			if (Blocksworld.selectedBunch == null) {
				BWLog.Info("[Console] Please select a model to export.");
				return;
			}
			Blocksworld.SaveMeshToFile(Blocksworld.selectedBunch);
		}
	}

    // Token: 0x0600288D RID: 10381 RVA: 0x0012A788 File Offset: 0x00128B88
    public void ConsoleWindow(int windowID)
	{
		this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition, new GUILayoutOption[0]);
		string text = string.Empty;
		int num = 0;
		for (int i = this._logs.Count - 1; i >= 0; i--)
		{
			BWConsole.Log log = this._logs[i];
			if (!this._hideDupes || !(log.message == text))
			{
				text = log.message;
				GUI.contentColor = BWConsole.logTypeColors[log.type];
				try
				{
					GUILayout.Label(text, new GUILayoutOption[0]);
				}
				catch
				{
					GUILayout.Label("Unable to display log message with stack trace:", new GUILayoutOption[0]);
				}
				GUILayout.Label(log.stackTrace, new GUILayoutOption[0]);
				num++;
				if (num > this.displayCapacity)
				{
					break;
				}
			}
		}
		GUILayout.EndScrollView();
		GUI.contentColor = Color.white;
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		if (GUILayout.Button(this._eraseLabel, new GUILayoutOption[0]))
		{
			this._logs.Clear();
		}
		this._hideDupes = GUILayout.Toggle(this._hideDupes, this._hideDupesLabel, new GUILayoutOption[]
		{
			GUILayout.ExpandWidth(false)
		});
		GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        this._command = GUILayout.TextField(this._command);
        if (GUILayout.Button("Submit", new GUILayoutOption[0]))
        {
            BWLog.Info("[Console] " + _command);
            CommandParse(_command);
            _command = "";
        }
        GUILayout.EndHorizontal();
		GUI.DragWindow(this._titleBarRect);
	}

    // Token: 0x0600288E RID: 10382 RVA: 0x0012A8F0 File Offset: 0x00128CF0
    public void HandleLog(string message, string stackTrace, LogType type)
	{
		if (this.ignoreWarnings && type == LogType.Warning)
		{
			return;
		}
		if (this._logs.Count >= this.messageCapacity)
		{
			this._logs.RemoveAt(0);
		}
		this._logs.Add(new BWConsole.Log
		{
			message = message,
			stackTrace = stackTrace,
			type = type
		});
	}

	// Token: 0x0400236C RID: 9068
	public KeyCode toggleKey = KeyCode.BackQuote;

	// Token: 0x0400236D RID: 9069
	public int messageCapacity = 2500;

	// Token: 0x0400236E RID: 9070
	public int displayCapacity = 500;

	// Token: 0x0400236F RID: 9071
	public bool ignoreWarnings = false;

	// Token: 0x04002370 RID: 9072
	private List<BWConsole.Log> _logs = new List<BWConsole.Log>();

	// Token: 0x04002371 RID: 9073
	private Vector2 _scrollPosition;

    // Token: 0x04002372 RID: 9074
    private bool _showConsole = false;

	// Token: 0x04002373 RID: 9075
	private bool _hideDupes;

	// Token: 0x04002374 RID: 9076
	private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
	{
		{
			LogType.Assert,
			Color.white
		},
		{
			LogType.Error,
			Color.red
		},
		{
			LogType.Exception,
			Color.red
		},
		{
			LogType.Log,
			Color.yellow
		},
		{
			LogType.Warning,
			Color.white
		}
	};

	// Token: 0x04002375 RID: 9077
	private const int margin = 20;

	// Token: 0x04002376 RID: 9078
	private Rect _windowRect = new Rect(20f, (float)Screen.height * 0.25f + 20f, (float)(Screen.width * 0.85f), (float)Screen.height * 0.75f - 40f);

	// Token: 0x04002377 RID: 9079
	private Rect _titleBarRect = new Rect(0f, 0f, 10000f, 20f);

	// Token: 0x04002378 RID: 9080
	private GUIContent _eraseLabel = new GUIContent("Erase All", "Clear the contents of the console.");

	// Token: 0x04002379 RID: 9081
	private GUIContent _hideDupesLabel = new GUIContent("Hide Duplicates", "Hide repeated messages.");

    private string _command = "";

	// Token: 0x020003A3 RID: 931
	private struct Log
	{
		// Token: 0x0400237A RID: 9082
		public string message;

		// Token: 0x0400237B RID: 9083
		public string stackTrace;

		// Token: 0x0400237C RID: 9084
		public LogType type;
	}
}
