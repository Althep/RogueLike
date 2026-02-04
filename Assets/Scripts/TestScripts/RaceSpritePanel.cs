using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static Defines;
public class RaceSpritePanel : MonoBehaviour
{
    [SerializeField] GameObject go;
    [SerializeField] Sprite[] elfSprite;
    [SerializeField] Dictionary<Races, Sprite[]> raceToBody = new Dictionary<Races, Sprite[]>();
    [SerializeField] SPUM_MatchingList machingList;
    [SerializeField] Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
    private void Awake()
    {
        foreach (Sprite s in elfSprite)
        {
            Debug.Log($"Spirte Name  : {s.name}");
        }
        if(machingList == null)
        {
            machingList = go.transform.GetComponentInChildren<SPUM_MatchingList>();
        }
        
        StartCoroutine("LoadBodys");
        
    }
    private void Start()
    {
        SetBodyToDungeonBorn();
    }
    IEnumerator LoadBodys()
    {
        Races[] races = (Races[])Enum.GetValues(typeof(Races));
        foreach (Races race in races)
        {
            if (race == Races.Default)
                continue;
            string raceName = race.ToString();
            Sprite[] sprite = Resources.LoadAll<Sprite>(raceName);
            raceToBody.Add(race, sprite);
        }
        SetBodyToDungeonBorn();
        yield return null;
    }


    public void SetBodyToDungeonBorn()
    {
        Races race = Races.DungeonBorn;
        string partName = "Body";
        if (machingList == null)
        {
            Debug.Log("MatchingList Null!");
            return;
        }
        List<MatchingElement> list = machingList.matchingTables;
        for(int i = 0; i < list.Count; i++)
        {
            Debug.Log($"PartName : {list[i].PartType}");
        }
        var elements = list.Where(x => x.PartType == partName);
        foreach(var el in elements)
        {
            Debug.Log(el.PartType);
        }
        var sprites = raceToBody[race];
        var matchedPairs =
        from elem in elements
        join sp in sprites on elem.Structure equals sp.name
        select new { elem, sp };

        foreach(var el in matchedPairs)
        {
            Debug.Log(el.sp.name);
        }
        foreach (var p in matchedPairs)
        {
            p.elem.renderer.sprite = p.sp;   // 스프라이트 교체
            
        }

    }

    public void SetSprite()
    {
        string partName = null; // Body,Armor,Weapon 적용할 대분류 부위 이름
        string partSub = null; // Sword,Axe,Sheild 무기의 구체적 타입
        string direction = null; // Left,Right 장비 착용방향
        string structure = null; // Weapons, 바디의 세부부위

        //PartType으로 대분류, 무기의경우 PartSubType, Diriection 정보 존재하나 PartSubType은 위치에 영향을 주지 않는것같음
        //타 부위 Structure정보, Armor의 경우 숄더 정보있음, 해당 파츠는ArmSelt에있음
        List<MatchingElement> list = machingList.matchingTables;

        var elements = list.Where(x => x.PartType == partName && x.PartSubType == partSub && x.Dir == direction && x.Structure == structure);

        var sps = sprites[partName];
    }
}