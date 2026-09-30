using Unity.Netcode;
using UnityEngine;

/// <summary>Runtime controls for starting the host or a loopback client.</summary>
public class Lab2NetworkUI : MonoBehaviour
{
    private NetworkManager FindManager()
    {
        NetworkManager manager = NetworkManager.Singleton;
        return manager != null ? manager : FindAnyObjectByType<NetworkManager>();
    }

    private void OnGUI()
    {
        NetworkManager manager = FindManager();
        if (manager != null && manager.IsListening)
            return;

        GUILayout.BeginArea(new Rect(12, 12, 340, 150), GUI.skin.box);
        GUILayout.Label("Multiplayer Lab 2");

        if (manager == null)
        {
            GUILayout.Label("No se encuentra NetworkManager en la escena.");
            GUILayout.Label("Abre SampleScene y revisa que NetworkManager esté activo.");
            GUILayout.EndArea();
            return;
        }

        if (GUILayout.Button("Iniciar Host"))
        {
            if (!manager.StartHost())
                Debug.LogError("No se pudo iniciar Host. Revisa NetworkTransport en NetworkManager.");
        }

        if (GUILayout.Button("Unirse a 127.0.0.1"))
        {
            if (!manager.StartClient())
                Debug.LogError("No se pudo conectar. Inicia Host primero en otra instancia.");
        }

        GUILayout.EndArea();
    }
}
