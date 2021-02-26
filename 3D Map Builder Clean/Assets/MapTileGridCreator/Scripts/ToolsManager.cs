﻿using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[ExecuteInEditMode]
public class ToolsManager : MonoBehaviour
{
    void OnEnable()
    {
        ToolsSupport.Hidden = true;
    }

    void OnDisable()
    {
        ToolsSupport.Hidden = false;
    }
}

public class ToolsSupport
{

    public static bool Hidden
    {
        get
        {
            Type type = typeof(Tools);
            FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
            return ((bool)field.GetValue(null));
        }
        set
        {
            Type type = typeof(Tools);
            FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
        }
    }
}