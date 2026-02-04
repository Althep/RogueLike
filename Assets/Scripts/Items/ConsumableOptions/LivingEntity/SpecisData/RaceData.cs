using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using UnityEditorInternal.VersionControl;

public class RaceData
{
    public Defines.Races race;

    public string disPlayName;

    public int bonusDice;

    public List<string> startItems = new List<string>();

    public List<Modifier> modifiers = new List<Modifier>();



}
