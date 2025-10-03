using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    
    [SerializeField] private float moveSpeed = 10f;
    private Vector3 direction;

    private NetworkVariable<PlayerData> playerData = new(
        new PlayerData
        {
            life = 100,
            isStunt = false
        },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private KeyCode leftKey = KeyCode.Q;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode upKey = KeyCode.Z;
    
    private NetworkVariable<int> randomNumber = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + " Random Number" + randomNumber.Value);
        };
        playerData.OnValueChanged += (PlayerData previousValue, PlayerData newValue) =>
        {
            Debug.Log(OwnerClientId + " life: " + newValue.life + " is stunt ? : " + newValue.isStunt + " " + newValue.message);
        };
    }

    private void Update()
    {
        if(!IsOwner) return;
        
        direction = Vector3.zero;

        if (Input.GetKey(leftKey))
        {
            direction.x = -1f;
        }
        if (Input.GetKey(rightKey))
        {
            direction.x = 1f;
        }
        if (Input.GetKey(downKey))
        {
            direction.z = -1f;
        }
        if (Input.GetKey(upKey))
        {
            direction.z = 1f;
        }

        direction = direction.normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            /*
            randomNumber.Value = Random.Range(0, 100);

            playerData.Value = new PlayerData()
            {
                life = Random.Range(0, 100),
                isStunt = false,
                message = "pseudo " + OwnerClientId
            };
            */
            
            //TestRpc(RpcTarget.Single(1,RpcTargetUse.Temp));
            
            InstanceTestRpc();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DestroyTestRpc();
        }
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void TestRpc(RpcParams rpcParams)
    {
        Debug.Log("test rcp "+ OwnerClientId + " message : " + rpcParams.Receive.SenderClientId);
    }

    private Transform spawnedObjectTransform;
    
    [Rpc(SendTo.Server)]
    void InstanceTestRpc()
    {
        spawnedObjectTransform = Instantiate(prefab, transform.position + new Vector3(0, Random.Range(-2, 2), 0), Quaternion.identity).transform;
        spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
    }
    
    [Rpc(SendTo.Server)]
    void DestroyTestRpc()
    {
        spawnedObjectTransform.GetComponent<NetworkObject>().Despawn();
    }
    
}

public struct PlayerData : INetworkSerializable
{
    public int life;
    public bool isStunt;
    public FixedString128Bytes message;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref life);
        serializer.SerializeValue(ref isStunt);
        serializer.SerializeValue(ref message);
    }
}