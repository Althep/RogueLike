using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IPotionEffect
{
    public void Apply();
}
public class PotionEffect : IPotionEffect
{

    public void Apply()
    {
        throw new NotImplementedException();
    }
}
