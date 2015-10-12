using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
public static class Utils
{
    public static void SetPosition(this Transform parent, float x, float y, float z)
    {
        var pos = parent.position;
        pos.Set(x, y, z);
        parent.position = pos;
    }

    public static void SetLocalPosition(this Transform parent, float x, float y, float z)
    {
        var pos = parent.localPosition;
        pos.Set(x, y, z);
        parent.localPosition = pos;
    }
}