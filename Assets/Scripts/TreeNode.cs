using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Click to claim the tree, then click three times as its owner to free it.</summary>
public class TreeNode : NetworkBehaviour
{
    [SerializeField] private int logs = 3;
    [SerializeField] private Color freeColor = Color.red;
    [SerializeField] private Color takenColor = Color.green;

    private readonly NetworkVariable<bool> isClaimed = new NetworkVariable<bool>(false);
    private Renderer treeRenderer;

    private void Awake()
    {
        treeRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Camera camera = Camera.main;
        if (mouse == null || camera == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        Ray ray = camera.ScreenPointToRay(mouse.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.gameObject != gameObject)
            return;

        if (!IsSpawned)
        {
            Debug.LogWarning("Clic detectado, pero el cubo no está en red. Pulsa Iniciar Host primero.");
            return;
        }

        Debug.Log("Clic detectado en el árbol.");

        if (!isClaimed.Value)
            ClaimServerRpc();
        else if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
            ChopServerRpc();
        else
            Debug.Log("Te ganaron, el árbol ya tiene dueño.");
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ClaimServerRpc(RpcParams rpcParams = default)
    {
        if (isClaimed.Value)
            return;

        if (!NetworkObject.IsOwnedByServer)
            return;

        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        isClaimed.Value = true;
        AnnounceCutsRpc(logs);
        Debug.Log("Server: el árbol ahora pertenece al jugador " + OwnerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ChopServerRpc(RpcParams rpcParams = default)
    {
        if (!isClaimed.Value || OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            Debug.LogWarning("Solo el dueño del árbol puede cortarlo.");
            return;
        }

        logs--;
        Debug.Log("Corte realizado. Quedan: " + logs);
        AnnounceCutsRpc(logs);

        if (logs > 0)
            return;

        NetworkObject.RemoveOwnership();
        isClaimed.Value = false;
        logs = 3;
        UpdateColor();
        AnnounceCutsRpc(logs);
        Debug.Log("Server: árbol agotado. Ahora está libre.");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AnnounceCutsRpc(int remaining)
    {
        logs = remaining;
    }

    public override void OnNetworkSpawn()
    {
        isClaimed.OnValueChanged += OnClaimedChanged;
        UpdateColor();
    }

    public override void OnNetworkDespawn()
    {
        isClaimed.OnValueChanged -= OnClaimedChanged;
    }

    private void OnClaimedChanged(bool previous, bool current)
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (treeRenderer != null)
            treeRenderer.material.color = isClaimed.Value ? takenColor : freeColor;
    }
}
