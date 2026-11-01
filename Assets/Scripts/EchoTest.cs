using Unity.Netcode;

using UnityEngine;

public class EchoTest : NetworkBehaviour {

    private const string Message = "Hello, Server";

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (NetworkManager.IsClient) {
            Debug.Log($"[Client]: Sending message `{Message}`");
            ServerEchoRpc(Message, NetworkManager.LocalClientId);

            MessageBox.Instance.ShowMessage("Client Connected!");
        }
    }

    [Rpc(SendTo.Server)]
    public void ServerEchoRpc(string message, ulong id) {
        Debug.Log($"[Server]: Received message from client {id}: `{message}`");
        ClientEchoRpc(message, RpcTarget.Single(id, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void ClientEchoRpc(string message, RpcParams clientParams) {
        Debug.Log($"[Client]: Received message back from server: `{message}`");
    }
}