using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public List<BaseCommand> commands = new List<BaseCommand>();

    private string commandInput = default;
    private bool showDebugConsole = false;
    private bool showCommandHelp = false;
    private Vector2 lastScrollPosition = Vector2.zero;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showDebugConsole = !showDebugConsole;
        }
    }

    #region Command Execution

    private void ExecuteCommand(BaseCommand command, object[] parameters)
    {
        string[] parts = command.actionName.Split('.');

        if (parts.Length != 2)
        {
            Debug.LogError($"Invalid Input - Input:{commandInput}");
            return;
        }

        if (!DebugHelper.TryGetClassType(parts[0], out Type classType))
        {
            Debug.LogError($"{parts[0]} class type not found!");
            return;
        }

        object magicClassObject = null;
        if (!DebugHelper.TryGetClassObject(classType, ref magicClassObject))
        {
            Debug.LogError($"Class not found (with default constructor not found)! Class type:{classType}");
            return;
        }

        if (!DebugHelper.TryGetMethodInfo(classType, parts[1], parameters.Length, out MethodInfo infoData))
        {
            Debug.LogError($"Method not found! Input:{commandInput}");
            return;
        }

        ParameterInfo[] parameterInfos = infoData.GetParameters();
        if (parameters.Length > 0 && parameterInfos.Length <= 0)
        {
            parameters = new object[] { };
        }

        commandInput = string.Empty;
        infoData.Invoke(magicClassObject, parameters);
    }

    private void PrepareForExecution()
    {
        string commandId = commandInput;

        if (string.IsNullOrEmpty(commandId))
        {
            Debug.LogError("Invalid input");
            return;
        }

        List<object> parameters = new List<object>();
        if (commandId.Contains(" "))
        {
            string[] parts = commandId.Split(' ');

            commandId = parts[0];

            int count = parts.Length;
            for (int i = 1; i < count; i++)
            {
                parameters.Add(parts[i]);
            }
        }

        BaseCommand command = commands.Find(c => c.commandId.Equals(commandId));

        if (command == null)
        {
            Debug.LogError($"Command with ID:{commandId} not found!");
            return;
        }

        ExecuteCommand(command, parameters.ToArray());
    }

    #endregion Command Execution

    #region Visual

    private void OnGUI()
    {
        if (!showDebugConsole)
        {
            return;
        }

        if (Event.current.Equals(Event.KeyboardEvent("return")))
        {
            PrepareForExecution();
        }

        DrawConsole();
    }

    private void DrawConsole()
    {
        float y = 0.0f;

        if (showCommandHelp)
        {
            DrawHelp(ref y);
        }

        GUI.Box(new Rect(0.0f, y, Screen.width, 35.0f), "");
        GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        commandInput = GUI.TextField(
            new Rect(10.0f, y + 5.0f,
                Screen.width - 20.0f, 20.0f),
            commandInput);

        GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        y += 35.0f;
        if (commandInput.Length > 0)
        {
            DrawSuggestions(y);
        }
    }

    private void DrawHelp(ref float y)
    {
        int count = commands.Count;
        if (count <= 0)
        {
            return;
        }

        GUI.Box(new Rect(0.0f, y, Screen.width, 125.0f), "");
        Rect viewPort = new Rect(0.0f, 0.0f, Screen.width - 30.0f, 20.0f * count);

        lastScrollPosition =
            GUI.BeginScrollView(
                new Rect(0.0f, y + 5.0f, Screen.width, 90.0f),
                lastScrollPosition, viewPort);

        for (int i = 0; i < count; i++)
        {
            BaseCommand command = commands[i];

            string info = command.GetInfo();
            Rect infoRect = new Rect(5.0f, 20.0f * i,
                viewPort.width - 100.0f, 20.0f);

            GUI.Label(infoRect, info);
        }

        GUI.EndScrollView();

        y += 125.0f;
    }

    private void DrawSuggestions(float y)
    {
        List<BaseCommand> suggestions = GetRelativeCommands();
        int count = suggestions.Count;
        if (count <= 0)
        {
            return;
        }

        GUI.Box(new Rect(0.0f, y, Screen.width, 125.0f), "");
        Rect viewPort = new Rect(0.0f, 0.0f, Screen.width - 30.0f, 20.0f * count);

        lastScrollPosition =
            GUI.BeginScrollView(
                new Rect(0.0f, y + 5.0f, Screen.width, 90.0f),
                lastScrollPosition, viewPort);
        GUIStyle style = GUI.skin.button;
        style.alignment = TextAnchor.MiddleLeft;
        style.margin.left = 15;

        GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        for (int i = 0; i < count; i++)
        {
            BaseCommand command = suggestions[i];
            string info = command.GetInfo();
            Rect infoRect = new Rect(5.0f, 20.0f * i,
                viewPort.width - 100.0f, 20.0f);

            if (GUI.Button(infoRect, info, style))
            {
                commandInput = command.commandFormat;
            }
        }

        GUI.EndScrollView();
    }

    #endregion Visual

    #region Default Commands

    public void ToggleHelp()
    {
        showCommandHelp = !showCommandHelp;
    }

    #endregion Default Commands

    #region Suggestion

    private List<BaseCommand> GetRelativeCommands()
    {
        List<BaseCommand> relativeCommands = new List<BaseCommand>();

        int count = commands.Count;
        for (int i = 0; i < count; i++)
        {
            if (!commands[i].commandId.Contains(commandInput))
            {
                continue;
            }

            relativeCommands.Add(commands[i]);
        }

        return relativeCommands;
    }

    #endregion Suggestion
}