using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : OrderableTarget
{
    public Transform center;
    // Start is called before the first frame update
    void Start()
    {
		InitWorkTargets();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
