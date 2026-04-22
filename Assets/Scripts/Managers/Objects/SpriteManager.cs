using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;
public class SpriteManager : MonoBehaviour
{
    Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    public static SpriteManager instance;
    [SerializeField] SpriteAtlas itemAtlas;
    private void Awake()
    {
        Init();
    }
    void Init()
    {
        if(SpriteManager.instance == null)
        {
            SpriteManager.instance = this;
        }

        PrepareCache();
    }
    private void PrepareCache()
    {
        if (itemAtlas == null) return;

        // 1. 아틀라스에 있는 모든 스프라이트를 가져올 배열 생성
        Sprite[] allSprites = new Sprite[itemAtlas.spriteCount];

        // 2. 배열에 스프라이트들을 꽉 채웁니다.
        itemAtlas.GetSprites(allSprites);

        // 3. 루프를 돌면서 (Clone)을 떼고 Dictionary에 넣습니다.
        foreach (Sprite s in allSprites)
        {
            // 유니티 내부에서 붙이는 (Clone) 문자열 제거
            string cleanName = s.name.Replace("_0(Clone)", "");

            if (!spriteCache.ContainsKey(cleanName))
            {
                spriteCache.Add(cleanName, s);
            }
        }

        Debug.Log($"아틀라스 캐싱 완료! 총 {spriteCache.Count}개의 아이콘 로드됨.");
    }

    public void SetSprite(SPUM_MatchingList matchingList,string partType)
    {

    }

    public void SetTileSprite()
    {

    }

    public void SetItemIcon(string itemName,Sprite targetSprite)
    {
        Sprite sprite = itemAtlas.GetSprite(itemName);
        if (sprite != null)
        {
            targetSprite = sprite;
        }
        else
        {
            Debug.Log($"{itemName} sprite Not In Atlas");
        }
    }

    public Sprite Get_Sprite(string itemName)
    {
        if (spriteCache.TryGetValue(itemName, out Sprite s))
        {
            return s;
        }
        
        Debug.Log($"[{itemName}] 이름의 스프라이트를 찾을 수 없습니다.");
        return null;
    }
}
