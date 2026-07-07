using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject bg;
    public GameObject win;
    public GameObject lose;
    public static GameManager instance;
    private void Awake() { instance = this; }

    [Server]
    public void EndGame()
    {
        NetworkManager.singleton.ServerChangeScene(
            SceneManager.GetActiveScene().name);
    }

    [Server]
    public void GameOver(string winnerSide)
    {
        RpcGameOver(winnerSide);

        Invoke(nameof(EndGame), 5f);
    }

    [ClientRpc]
    void RpcGameOver(string winner)
    {
        bg.SetActive(true);

        Player player = NetworkClient.localPlayer.GetComponent<Player>();

        if (player.playerSide == winner)
            win.SetActive(true);
        else
            lose.SetActive(true);
    }
}