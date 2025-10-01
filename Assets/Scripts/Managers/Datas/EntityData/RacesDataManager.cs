using UnityEngine;
using System.Collections.Generic;
public class RacesDataManager 
{
    public Dictionary<Defines.Races, RaceData> raceDatas; 

    public Dictionary<Defines.Races, List<Defines.Jobs>> raceJobList = new Dictionary<Defines.Races, List<Defines.Jobs>>();
    public Dictionary<Defines.Jobs, List<string>> startItems = new Dictionary<Defines.Jobs, List<string>>();
    public void Init()
    {

    }
}
