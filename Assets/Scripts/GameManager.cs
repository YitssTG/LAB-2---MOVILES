using Unity.Netcode;
using UnityEngine;

/// <summary>Spawns one player avatar for each connected client.</summary>
public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    private Camera sceneCamera;
    private AudioListener sceneAudioListener;
    private Lab2NetworkUI networkUI;

    private void Awake()
    {
        sceneCamera = Camera.main;
        sceneAudioListener = sceneCamera != null ? sceneCamera.GetComponent<AudioListener>() : null;
        networkUI = FindAnyObjectByType<Lab2NetworkUI>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
            return;
        foreach (ulong clientId in manager.ConnectedClientsIds)
            SpawnPlayer(clientId);
        manager.OnClientConnectedCallback += SpawnPlayer;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;

        SimplePlayer localController = FindAnyObjectByType<SimplePlayer>();
        if (localController != null && localController.IsOwner)
            localController.ResetLocalCamera();

        if (networkUI != null)
            networkUI.enabled = true;
    }

    private void SpawnPlayer(ulong clientId)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (!IsServer || playerPrefab == null || manager == null ||
            !manager.ConnectedClients.ContainsKey(clientId) || manager.ConnectedClients[clientId].PlayerObject != null)
            return;

        Vector3 spawnPosition = clientId == NetworkManager.ServerClientId
            ? new Vector3(-2f, 1f, -1f)
            : new Vector3(2f, 1f, -1f);

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.SpawnAsPlayerObject(clientId, true);
        if (clientId == manager.LocalClientId)
        {
            SimplePlayer playerController = player.GetComponent<SimplePlayer>();
            playerController.SetLocalCamera(sceneCamera);
            if (sceneAudioListener != null)
                sceneAudioListener.enabled = false;
        }
    }
}
