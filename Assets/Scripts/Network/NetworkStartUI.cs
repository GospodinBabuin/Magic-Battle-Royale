using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkStartUI : NetworkBehaviour
    {
        [SerializeField] private Button startServerButton;
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button startClientButton;

        private void Start()
        {
            startServerButton.onClick.AddListener(StartServer);
            startHostButton.onClick.AddListener(StartHost);
            startClientButton.onClick.AddListener(StartClient);
        }

        private void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            HideButtons();
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            HideButtons();
        }
    
        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            HideButtons();
        }

        private void HideButtons() => gameObject.SetActive(false);
    }
}
