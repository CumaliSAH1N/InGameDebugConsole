using UnityEngine;

public abstract class BaseCommand : ScriptableObject
{
    public string commandId = default;
    public string commandDescription = default;
    public string commandFormat = default;
    public string actionName = default;

    public virtual string GetInfo() => $"{commandFormat} - {commandDescription}";
}