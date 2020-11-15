using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public List<BaseCommand> commands = new List<BaseCommand>();

    public string commandInput = default;
    public bool showDebugConsole = false;
    public bool showCommandHelp = false;
    private Vector2 lastScrollPosition = Vector2.zero;
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showDebugConsole = !showDebugConsole;
        }
    }

    #region Command Execution

    private void ExecuteCommand(BaseCommand c, object[] parameters)
    {
        string[] parts = c.actionName.Split('.');

        if (parts.Length != 2)
        {
            Debug.LogError($"Invalid Input - Input:{commandInput}");
            return;
        }

        if (!DebugHelper.GetClassType(parts[0], out Type classType))
        {
            Debug.LogError($"{parts[0]} class type not found!");
            return;
        }

        object magicClassObject = null;
        if (!DebugHelper.GetClassObject(classType, ref magicClassObject))
        {
            Debug.LogError($"Class not found (with default constructor not found)! Class type:{classType}");
            return;
        }

        if (!DebugHelper.GetMethodInfo(classType, parts[1], parameters.Length, out MethodInfo infoData))
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

    private void Execute()
    {
        if (string.IsNullOrEmpty(commandInput))
        {
            return;
        }
        
        string commandId = commandInput;

        List<object> parameters = new List<object>();
        if (commandInput.Contains(" "))
        {
            string[] parts = commandInput.Split(' ');

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
            Execute();
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

            string info = $"{command.commandFormat} - {command.commandDescription}";
            Rect infoRect = new Rect(5.0f, 20.0f * i,
                viewPort.width - 100.0f, 20.0f);
          
            GUI.Label(infoRect, info);
        }

        GUI.EndScrollView();

        y += 125.0f;
    }

    #endregion Visual
    
    #region Default

    public void ToggleHelp()
    {
        showCommandHelp = !showCommandHelp;
    }

    #endregion Default
}