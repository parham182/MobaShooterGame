using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager instance;
    public List<IDamageable> targets = new List<IDamageable>();

    [Header("Players")]
    public Transform blueTeamSpawnPoint;
    public Transform redTeamSpawnPoint;

    [Header("Creeps")]
    public Transform blueTeamCreepSpawnPoint;
    public Transform redTeamCreepSpawnPoint;
    public GameObject creepPrefab;

    private float timer;

    private void Awake()
    {
        instance = this;
    }

    public override void OnStartServer()
    {
        StartCoroutine(Spawn());
        
    }

    [Server]
    IEnumerator Spawn()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject creepObject = Instantiate(creepPrefab, blueTeamCreepSpawnPoint.position, Quaternion.identity);
            Creep creep = creepObject.GetComponent<Creep>();
            AddDamageable(creep);
            creep.creepSide = "blue";
            NetworkServer.Spawn(creepObject);

            creepObject = Instantiate(creepPrefab, redTeamCreepSpawnPoint.position, Quaternion.identity);
            creep = creepObject.GetComponent<Creep>();
            AddDamageable(creep);
            creep.creepSide = "red";
            NetworkServer.Spawn(creepObject);

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(30f);
        StartCoroutine(Spawn());
    }

    public void AddDamageable(IDamageable damageable)
    {
        targets.Add(damageable);
    }

    public void RemoveDamageable(IDamageable damageable)
    {
        targets.Remove(damageable);
    }
}
