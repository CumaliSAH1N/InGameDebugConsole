using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

public static class DebugHelper
{
    #region Command Info
    public static bool TryGetClassType(string className, out Type classType)
    {
        classType = Type.GetType(className);
        return classType != null;
    }

    public static bool TryGetMethodInfo(Type classType, string info, int parameterCount, out MethodInfo infoData)
    {
        string methodName = info;
        if (info.Contains(" "))
        {
            string[] sParts = info.Split(' ');
            methodName = sParts[0];
        }

        List<MethodInfo> allMethods = classType.GetMethods().ToList();
        List<MethodInfo> foundMethodInfos = allMethods.FindAll(m => m.Name.Equals(methodName));

        infoData = foundMethodInfos.Find(m => m.GetParameters().Length.Equals(parameterCount));

        return infoData != null;
    }

    public static bool TryGetClassObject(Type classType, ref object classObject)
    {
        if (classType.BaseType == typeof(MonoBehaviour))
        {
            classObject = Object.FindObjectOfType(classType);

            if (classObject != null)
            {
                return true;
            }

            Debug.Log($"{classType} has been created!");
            GameObject go = new GameObject(classType.Name);
            classObject = go.AddComponent(classType);
        }
        else
        {
            ConstructorInfo magicConstructor = classType.GetConstructor(Type.EmptyTypes);
            if (magicConstructor == null)
            {
                Debug.Log($"Could not find default constructor on class {classType}");
                return false;
            }

            Debug.Log($"A new Instance of {classType} has been created");
            classObject = magicConstructor.Invoke(new object[] { });
        }

        return true;
    }

    #endregion Command Info
}