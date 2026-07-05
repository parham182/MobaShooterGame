using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    int playerCount = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);

        Player p = player.GetComponent<Player>();

        playerCount++;
        p.playerSide = (playerCount % 2 == 0) ? "red" : "blue";

        NetworkServer.AddPlayerForConnection(conn, player);

        p.Respawn();
        SpawnManager.instance.AddDamageable(p);
    }
}
