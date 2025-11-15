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
}