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
using System.Text;

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
        _codeCopy.gameObject.SetActive(false);
        _startServer.onClick.AddListener(StartServer);
        _startClient.onClick.AddListener(StartClient);

        NetworkManager.Singleton.OnTransportFailure += ShowTransportFailure;
        _transport.OnTransportEvent += ShowErrorEvent;
#endif
    }

    private async void StartServer() {
#if !UNITY_SERVER
        _codeInput.gameObject.SetActive(false);
        _startClient.gameObject.SetActive(false);
        _startServer.gameObject.SetActive(false);
        _codeCopy.interactable = false;
        _codeCopy.gameObject.SetActive(true);
#endif

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData data = allocation.ToRelayServerData(GetConnectionType());
            _transport.SetRelayServerData(data);

            if (!NetworkManager.Singleton.StartServer()) {
                Debug.LogError("[Lobby] Failed to start server...", this);
                return;
            }
            Debug.Log($"[Lobby] Server started on {data.Endpoint.Address}");
            Debug.Log($"[Lobby] Server Join Code: {joinCode}");

#if !UNITY_SERVER
            CopyCode(joinCode);
            _codeOutput.text = joinCode;
            _codeCopy.interactable = true;
            MessageBox.Instance.ShowMessage("Server Started...");
#endif
        } catch (Exception e) {
            string error = string.Empty;
            GetFullException(ref error, e);
            Debug.LogError($"[Lobby] Relay server start failed: {error}", this);
#if !UNITY_SERVER
            ErrorBox.Instance.ShowError($"[Lobby]: Failed to start lobby...\n{error}");
#endif
        }

    }

    private async void StartClient() {
        _codeInput.gameObject.SetActive(false);
        _codeCopy.gameObject.SetActive(false);
        _startClient.gameObject.SetActive(false);
        _startServer.gameObject.SetActive(false);

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            string joinCode = _codeInput.text.Trim();
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData data = allocation.ToRelayServerData(GetConnectionType());
            _transport.SetRelayServerData(data);

            if (!NetworkManager.Singleton.StartClient()) {
                Debug.LogError("[Lobby] Failed to start client...", this);
                return;
            }
            Debug.Log($"[Lobby] Client connected to {data.Endpoint.Address}");
        } catch (Exception e) {
            Debug.LogError($"[Lobby] Relay client start failed: {e.Message}", this);
            string error = string.Empty;
            GetFullException(ref error, e);
            ErrorBox.Instance.ShowError(error);
        }
    }

    private void ShowTransportFailure() {
        ErrorBox.Instance.ShowError($"Unity transport failure (likely failed to find server)");
    }

    private void ShowErrorEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime) {
        int seconds = Mathf.RoundToInt(receiveTime);
        string payloadString = payload.Count > 0 ? Encoding.UTF8.GetString(payload) : "None";
        if (NetworkManager.Singleton.IsClient) {
            if (eventType == NetworkEvent.TransportFailure) {
                ErrorBox.Instance.ShowError($"Unity transport failure:\nLikely failed to find any server\nTime of failure: {seconds / 60}m:{seconds % 60}s");
                ErrorBox.Instance.ShowError($"\nPayload: {payloadString}");
            } else if (eventType == NetworkEvent.Disconnect) {
                ErrorBox.Instance.ShowError($"Unity transport disconnect\nLikely that server died or inaccessible?\nTime of failure: {seconds / 60}m:{seconds % 60}s");
                ErrorBox.Instance.ShowError($"\nPayload: {payloadString}");
            }
        } else if (NetworkManager.Singleton.IsServer) {
            if (eventType == NetworkEvent.TransportFailure) {
                ErrorBox.Instance.ShowError($"Unity transport failure:\nCould server could not start for some reason?\nTime of failure: {seconds / 60}m:{seconds % 60}s");
                ErrorBox.Instance.ShowError($"\nPayload: {payloadString}");
            } else if (eventType == NetworkEvent.Disconnect) {
                ErrorBox.Instance.ShowError($"Unity transport disconnect (Lost connection?)\nTime of failure: {seconds / 60}m:{seconds % 60}s");
                ErrorBox.Instance.ShowError($"\nPayload: {payloadString}");
            }
        }
    }

    private void GetFullException(ref string message, Exception e, int depth = 0) {
        if (depth == 0) {
            message += $"[Lobby Error] {e.Message}\nIn: {e.StackTrace}";
        } else {
            message += "\n------------------------------";
            message += $"\n{e.Message}\nIn: {e.StackTrace}";
        }

        if (e.InnerException != null) {
            GetFullException(ref message, e.InnerException, depth += 1);
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

    private string GetConnectionType() {
#if !UNITY_WEBGL
        return "dtls";
#else
        return "wss";
#endif
    }
}