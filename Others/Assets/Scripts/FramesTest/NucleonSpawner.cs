using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NucleonSpawner : MonoBehaviour
{
    public float m_timeBetweenSpawns;
    public float m_spawnDistance;
    public Nucleon[] m_nucleonPrefabs;


    private float m_timeSinceLastSpawn;
    
    private void FixedUpdate()
    {
        m_timeSinceLastSpawn += Time.deltaTime;
        if (m_timeBetweenSpawns >= m_timeBetweenSpawns)
        {
            m_timeSinceLastSpawn -= m_timeBetweenSpawns;
            SpawnNucleon();
        }
    }

    private void SpawnNucleon()
    {
        Nucleon prefab = m_nucleonPrefabs[Random.Range(0, m_nucleonPrefabs.Length)];
        Nucleon spawn = Instantiate<Nucleon>(prefab);
        spawn.transform.localPosition = Random.onUnitSphere * m_spawnDistance;
    }
}
