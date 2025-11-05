using System;
using UnityEngine;

public class MiscItem : ItemBase
{
    public Defines.MiscType miscType;

    public override Enum GetSpecificType()
    {
        return miscType;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
