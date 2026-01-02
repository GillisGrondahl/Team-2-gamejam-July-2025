
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{

    public static void PrintList<T>(List<T> list)
    {
        var test = string.Join(" | ", list);

        Debug.Log(test);
    }
}
