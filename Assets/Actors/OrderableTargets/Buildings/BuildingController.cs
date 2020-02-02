using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : OrderableTarget
{
    public Transform center;
    public float orbitRadius;

    // Start is called before the first frame update
    void Start()
    {
		InitWorkTargets();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetOrbitRadius()
    {
        return orbitRadius;
    }

    public Transform GetOrbitCenter()
    {
        return center;
    }
}
