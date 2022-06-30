using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private GameObject _spawnedPlayerPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
    }

    // Update is called once per frame
    void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying to connect to the server");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Successfully connected to the server");
        base.OnConnectedToMaster();
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("I am connected to the room");
        base.OnJoinedRoom();
        _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }
    
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(_spawnedPlayerPrefab);
    }
    
}
