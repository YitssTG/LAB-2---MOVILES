using Unity.Netcode;
using UnityEngine;

/// <summary>Ownership-based resource sample included in the original Lab 2 repository.</summary>
public class NetworkResource : NetworkBehaviour
{
    [SerializeField] private Color freeColor = Color.green;
    [SerializeField] private Color takenColor = Color.red;

    private Renderer resourceRenderer;
    private readonly NetworkVariable<bool> isClaimed = new NetworkVariable<bool>(false);

    private void Awake()
    {
        resourceRenderer = GetComponent<Renderer>();
    }

    private void OnMouseDown()
    {
        if (!IsSpawned)
            return;

        if (!isClaimed.Value)
            ClaimServerRpc();
        else
            Debug.Log("Ya tiene dueño.");
    }

    [Rpc(SendTo.Server)]
    private void ClaimServerRpc(RpcParams rpcParams = default)
    {
        if (isClaimed.Value)
            return;

        isClaimed.Value = true;
        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        UpdateColor();
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
        if (resourceRenderer != null)
            resourceRenderer.material.color = isClaimed.Value ? takenColor : freeColor;
    }
}
