using UnityEngine;

public class RaceSelectCell : SelectSubCell
{
    Defines.Races selectRace;

    public override void SetMyType<T>(T type)
    {
        if(type is Defines.Races race)
        {
            selectRace = race;
        }
    }
}
