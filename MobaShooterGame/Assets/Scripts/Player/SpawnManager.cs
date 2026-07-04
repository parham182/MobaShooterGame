using Mirror;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager instance;

    [Header("Players")]
    public Transform blueTeamSpawnPoint;
    public Transform redTeamSpawnPoint;

    [Header("Towers")]
    public Transform blueTeamTowerSpawnPoint;
    public Transform redTeamTowerSpawnPoint;
    public GameObject towerPrefab;

    [Header("Creeps")]
    public Transform blueTeamCreenSpawnPoint;
    public Transform redTeamCreenSpawnPoint;

    private void Awake()
    {
        instance = this;
    }

    public override void OnStartServer()
    {
        // GameObject towerBlue = Instantiate(towerPrefab, blueTeamTowerSpawnPoint.position, Quaternion.identity);
        // GameObject towerRed = Instantiate(towerPrefab, redTeamTowerSpawnPoint.position, Quaternion.identity);

        // towerBlue.GetComponent<Tower>().towerSide = 0;
        // towerRed.GetComponent<Tower>().towerSide = 1;

        // NetworkServer.Spawn(towerBlue);
        // NetworkServer.Spawn(towerRed);
    }
}
