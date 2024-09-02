using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Manager
{
    public virtual IEnumerator Instantiate()
    {
        yield return null;
    }
}
