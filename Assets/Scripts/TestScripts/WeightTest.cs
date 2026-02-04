using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
public class WeightTest : UI_Base
{
    List<WeightKeyPair> keyPair = new List<WeightKeyPair>();
    private void Awake()
    {
        
        MakeData();
    }
    void Start()
    {
        AddUIEvent(this.gameObject, OnClickAction, Defines.UIEvents.Click);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MakeData()
    {
        WeightKeyPair keypairA = new WeightKeyPair();
        WeightKeyPair keypairB = new WeightKeyPair();
        WeightKeyPair keypairC = new WeightKeyPair();
        WeightKeyPair keypairD = new WeightKeyPair();
        WeightKeyPair keypairE = new WeightKeyPair();

        keypairA.key = "A"; keypairA.cumulativeWeight = 0.5f;
        keypairB.key = "B"; keypairB.cumulativeWeight = 1f;
        keypairC.key = "C"; keypairC.cumulativeWeight = 2.5f;
        keypairD.key = "D"; keypairD.cumulativeWeight = 3f;
        keypairE.key = "E"; keypairE.cumulativeWeight = 4.5f;
        keyPair.Add(keypairE);
        keyPair.Add(keypairD);
        keyPair.Add(keypairC);
        keyPair.Add(keypairB);
        keyPair.Add(keypairA);
    }
    
    public void OnClickAction(PointerEventData pve)
    {
        Utils.GetRandomIDToWeight(keyPair);
    }
}
