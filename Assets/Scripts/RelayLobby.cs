// #define UNITY_SERVER
using UnityEngine;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using System;

using TMPro;
using UnityEngine.UI;

public class RelayLobby : MonoBehaviour {
    [SerializeField] private TMP_Text _codeOutput;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private UnityTransport _transport;
    [SerializeField] private Button _codeCopy;
    [SerializeField] private Button _startServer;
    [SerializeField] private Button _startClient;

    private void Start() {
        _transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        NetworkManager.Singleton.OnClientConnectedCallback += OnJoin;

#if UNITY_SERVER
        StartServer();
#else
        _codeCopy.onClick.AddListener(() => CopyCode(_codeOutput.text));
        _startServer.onClick.AddListener(StartServer);
        _startClient.onClick.AddListener(StartClient);
#endif
    }

    private async void StartServer() {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData data = allocation.ToRelayServerData("dtls");
            _transport.SetRelayServerData(data);

            if (!NetworkManager.Singleton.StartServer()) {
                Debug.LogError("[Lobby] Failed to start server...", this);
                return;
            }
            Debug.Log($"[Lobby] Server started on {data.Endpoint.Address}");
            Debug.Log($"[Lobby] Server Join Code: {joinCode}");

#if UNITY_CLIENT || UNITY_EDITOR
            CopyCode(joinCode);
            _codeOutput.text = joinCode;
#endif
        } catch (Exception e) {
            Debug.LogError($"[Lobby] Relay server start failed: {e.Message}", this);
        }
    }

    private async void StartClient() {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            string joinCode = _codeInput.text.Trim();
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData data = allocation.ToRelayServerData("dtls");
            _transport.SetRelayServerData(data);

            if (!NetworkManager.Singleton.StartClient()) {
                Debug.LogError("[Lobby] Failed to start client...", this);
                return;
            }
            Debug.Log($"[Lobby] Client connected to {data.Endpoint.Address}");
        } catch (Exception e) {
            Debug.LogError($"[Lobby] Relay client start failed: {e.Message}", this);
        }
    }

    private void OnJoin(ulong client) {
        if (NetworkManager.Singleton.IsConnectedClient) {
            _codeInput.gameObject.SetActive(false);
        }
    }

    private void CopyCode(string code) {
        TextEditor editor = new TextEditor();
        editor.text = code.Trim().ToUpper();
        editor.SelectAll();
        editor.Copy();
    }
}