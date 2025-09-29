using UnityEngine;
using System.Collections.Generic;
public class RacesDataManager 
{
    public Dictionary<Defines.Races, RaceData> raceDatas; 
    public Dictionary<Defines.Races, List<AdvanceType>> raceAdvances;
    public Dictionary<Defines.Races, List<PaneltyType>> racePaneltys;
    public Dictionary<Defines.Races, List<Defines.Jobs>> raceJobList = new Dictionary<Defines.Races, List<Defines.Jobs>>();
    public Dictionary<Defines.Jobs, List<string>> jobEquipList = new Dictionary<Defines.Jobs, List<string>>();
    public void Init()
    {

    }
}
