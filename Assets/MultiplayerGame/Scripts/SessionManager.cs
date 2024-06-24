using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using LitJson;

public class SessionManager : NetworkBehaviour
{
    private static SessionManager _instance = null;


    public static SessionManager singleton
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SessionManager>();
            }

            return _instance;
        }
    }

    public void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.StartServer();
    }

    private void OnClientConnect(ulong clientId)
    {
        ulong[] target = new ulong[1];
        target[0] = clientId;
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = target;
        OnClientConnectedClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void OnClientConnectedClientRpc(ClientRpcParams rpcParams = default)
    {
        long accountId = 0;
        SpawnCharacterServerRpc(accountId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRpc(long accountId, ServerRpcParams serverRpcParams = default)
    {
        Character prefab = PrefabManager.singleton.GetCharacterPrefab("Bot");

        if (prefab != null)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
            Character character = Instantiate(prefab, pos, Quaternion.identity);
            character.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

            Dictionary<string, int> items = new Dictionary<string, int>
                { { "Rifle", 1 }, { "RifleBullet", 1000 }, { "Pistol", 1 }, { "PistolBullet", 1000 } };

            List<string> itemIds = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                itemIds.Add(System.Guid.NewGuid().ToString());
            }

            string itemJson = JsonMapper.ToJson(items);
            string itemIdJson = JsonMapper.ToJson(itemIds);

            character.InitializeServer(items, itemIds, serverRpcParams.Receive.SenderClientId);
            character.InitializeClientRpc(itemJson, itemIdJson, serverRpcParams.Receive.SenderClientId);
        }
    }


    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}