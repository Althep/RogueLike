using UnityEngine;
using System;
using System.Collections.Generic;
public class SpriteManager : MonoBehaviour
{
    Dictionary<string, Sprite> partKeySpritePair = new Dictionary<string, Sprite>();
    public static SpriteManager instance;

    private void Awake()
    {
        Init();
    }
    void Init()
    {
        instance = this;
    }

    public Sprite GetSprite(string key)
    {
        if (partKeySpritePair.ContainsKey(key))
        {
            return partKeySpritePair[key];
        }
        Debug.Log($"SpriteKey Didn't Contain Key : {key}");
        return null;
    }
    public void SetSprite(SPUM_MatchingList matchingList,string partType)
    {

    }

    public void SetTileSprite()
    {

    }
}
