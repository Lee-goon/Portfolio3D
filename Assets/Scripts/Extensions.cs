using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions 
{
    public static WantType MakeSingleton<WantType>(this WantType target, ref WantType location)
    {
        if(location == null)
        {
            location = target;
        }
        else
        {
            Object targetObject = target as Object;
            if (targetObject)
            {
                GameObject.Destroy(targetObject);
            }
        }

        return location;
    }
}
