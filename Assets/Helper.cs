using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Helper
{
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return (Vector2)vector;
    }
    public static List<Vector2> ToPosition<T>(this List<T> roomList) where T : MonoBehaviour
    {
        List<Vector2> result = new List<Vector2>();
        foreach (T go in roomList)
        {
            result.Add(go.transform.position.ToVector2());
        }
        return result;
    }
    public static Vector2 Floor(this Vector2 vector)
    {
        vector.x = Mathf.Floor(vector.x);
        vector.y = Mathf.Floor(vector.y);
        return vector;
    }

    public static float Sum(this Vector2 vector)
    {
        return Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
    }

}
