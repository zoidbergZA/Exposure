using UnityEngine;
using System.Collections;

public static class WindowIDManager
{
    static int id;

    public static int GetID ()
    {
        id++;
        return id;
    }
}
