using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>Owner-controlled, server-validated movement from the Lab 2 sample.</summary>
public class SimplePlayer : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    private Camera followCamera;
    private AudioListener followAudioListener;

    public void SetLocalCamera(Camera cameraToFollow)
    {
        followCamera = cameraToFollow;
        followAudioListener = cameraToFollow != null ? cameraToFollow.GetComponent<AudioListener>() : null;
        if (followCamera != null)
            followCamera.enabled = true;
        if (followAudioListener != null)
            followAudioListener.enabled = true;
    }

    public void ResetLocalCamera()
    {
        if (followCamera != null)
        {
            followCamera.transform.SetPositionAndRotation(new Vector3(0f, 4.5f, -8f), Quaternion.Euler(20f, 0f, 0f));
            followCamera.enabled = true;
        }

        if (followAudioListener != null)
            followAudioListener.enabled = true;
        followCamera = null;
        followAudioListener = null;
    }

    public override void OnGainedOwnership()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (!camera.CompareTag("MainCamera"))
                continue;

            SetLocalCamera(camera);
            if (followAudioListener != null)
                followAudioListener.enabled = false;
            break;
        }
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned)
            return;

        if (followCamera != null)
        {
            followCamera.transform.position = transform.position + new Vector3(0f, 4.5f, -8f);
            followCamera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        float horizontal = 0f;
        float vertical = 0f;
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                       - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            vertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                     - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        }
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }
        if (horizontal == 0f && vertical == 0f)
            return;

        float x = horizontal * speed * Time.deltaTime;
        float z = vertical * speed * Time.deltaTime;
        transform.position += new Vector3(x, 0f, z);
    }

    private void Awake()
    {
        // El prefab puede ser inválido si se edita NetworkTransform a mano en YAML.
        // Forzamos autoridad del dueño antes de que el objeto aparezca en la red.
        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        if (networkTransform != null)
            networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
    }

    public override void OnNetworkDespawn()
    {
        ResetLocalCamera();
    }
}
