using UnityEngine;

public class JobSelectCell : SelectSubCell
{
    Defines.Jobs selectJob;


    public override void SetMyType<T>(T type)
    {
        if(type is Defines.Jobs job)
        {
            selectJob = job;
        }
    }
}
