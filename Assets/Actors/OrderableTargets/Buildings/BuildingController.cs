using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : OrderableTarget
{
	private Health m_health;
	public int healPerWork;

    public Transform center;
    public float orbitRadius;

    // Start is called before the first frame update
    void Start()
    {
		InitWorkTargets();
		m_health = GetComponent<Health>();
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

	public override void OnWork( CuboController user )
	{
		m_health.Heal( healPerWork );
	}
}
