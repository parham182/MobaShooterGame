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
        InvokeRepeating(nameof(SpawnCreeps), 10f, 30f);
    }

    [Server]
    void SpawnCreeps()
    {
        GameObject creepObject = Instantiate(creepPrefab, blueTeamCreepSpawnPoint.position, Quaternion.identity);
        AddDamageable(creepObject.GetComponent<IDamageable>());
        Creep creep = creepObject.GetComponent<Creep>();
        creep.creepSide = "blue";
        NetworkServer.Spawn(creepObject);

        creepObject = Instantiate(creepPrefab, redTeamCreepSpawnPoint.position, Quaternion.identity);
        AddDamageable(creepObject.GetComponent<IDamageable>());
        creep = creepObject.GetComponent<Creep>();
        creep.creepSide = "red";
        NetworkServer.Spawn(creepObject);
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
