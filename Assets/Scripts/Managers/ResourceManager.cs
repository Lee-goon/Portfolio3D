using ResourceEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Manager
{
    static Dictionary<ResourceEnum.Prefab,     GameObject> prefabDictionary;
    static Dictionary<ResourceEnum.BGM,        AudioClip> bgmDictionary;
    static Dictionary<ResourceEnum.SFX,        AudioClip> sfxDictionary;
    static Dictionary<ResourceEnum.Animation,  AnimationClip> animDictionary;

    public override IEnumerator Instantiate()
    {
        int resourceAmount = 0;
        resourceAmount += ResourcePath.prefabPathArray.Length;
        resourceAmount += ResourcePath.bgmPathArray.Length;
        resourceAmount += ResourcePath.sfxPathArray.Length;
        resourceAmount += ResourcePath.animPathArray.Length;
        return base.Instantiate();
    }
}
