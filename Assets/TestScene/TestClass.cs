using System.Reflection;
using UnityEngine;

public class TestClass : MonoBehaviour
{
    public void Show()
    {
        Debug.Log($"{MethodBase.GetCurrentMethod().Name} triggered");
    }

    public void SetValue(object value)
    {
        Debug.Log(
            int.TryParse(value.ToString(), out int num)
                ? $"{num} has been set" : $"Type mismatch");
    }
}
