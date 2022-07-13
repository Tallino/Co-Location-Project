using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoLocationSynchronizer : MonoBehaviourPunCallbacks, XRIDefaultInputActions.ISynchronizeActions
{
    private XRIDefaultInputActions _defaultInputActions;
    private const byte SendIDForSync = 1;
    private const byte SendPositionForCoLocation = 2;
    private const byte ResetID = 3;
    private int _idOfPlayerToBePositioned;

    public void Start()
    {
        _defaultInputActions = new XRIDefaultInputActions();
        _defaultInputActions.Synchronize.SetCallbacks(this);
        _defaultInputActions.Synchronize.Enable();
    }

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        //Event triggered by grip button: received the id of the player who wants to be positioned
        if (photonEvent.Code == SendIDForSync && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
            gameObject.GetComponent<NetworkPlayer>().SetStateHasChanged(true);
        }


        //Event triggered by bunny hand gesture: received the information about the origin system of the master client
        if (photonEvent.Code == SendPositionForCoLocation && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned && gameObject.GetPhotonView().IsMine)
        {
            var data = (object[]) photonEvent.CustomData;
   
            //Receiving Mean Hand information from the master client
            var otherMeanHandPosition = (Vector3)data[0];
            var otherMeanHandRotation = (Quaternion)data[1];

            //Finding Mean Hand information of the player to be positioned (ourselves)
            var myMeanHandPosition = Vector3.Lerp(GameObject.Find("LeftHandAnchor").transform.position, GameObject.Find("RightHandAnchor").transform.position, 0.5f);
            var myMeanHandRotation = Quaternion.Lerp(GameObject.Find("LeftHandAnchor").transform.rotation, GameObject.Find("RightHandAnchor").transform.rotation, 0.5f);

            //CALCULATING ROTATION
            var deltaRotation = otherMeanHandRotation.eulerAngles.y - myMeanHandRotation.eulerAngles.y;

            //CALCULATING POSITION
            var deltaPosition = otherMeanHandPosition - myMeanHandPosition;
            deltaPosition.y = 0;
            
            //APPLYING ROTATION AND POSITION
            GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(myMeanHandPosition, Vector3.up, 180 + deltaRotation);
            GameObject.Find("OVRCameraRig").gameObject.transform.position += deltaPosition;
            
            //Resetting ID of player to be positioned
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(ResetID, 0, raiseEventOptions, SendOptions.SendReliable);
        }

        //Event triggered at the end of the above event
        if (photonEvent.Code == ResetID && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = (int)photonEvent.CustomData;
            gameObject.GetComponent<NetworkPlayer>().SetStateHasChanged(true);
            Debug.Log("Co-Location ended");
        }
    }

    public void OnSendData(InputAction.CallbackContext context)
    {
        if (!PhotonNetwork.IsMasterClient && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = gameObject.GetPhotonView().ViewID;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(SendIDForSync, gameObject.GetPhotonView().ViewID, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    // Function triggered with hand gesture
    public void CoLocate()
    {
        if(_idOfPlayerToBePositioned == 0)
            return;
        
        Debug.Log("Co-Location initialized");

        //Sending master client origin system information to the player who wants to be positioned
        if (PhotonNetwork.IsMasterClient && gameObject.GetPhotonView().IsMine)
        {
            var meanHandPosition = Vector3.Lerp(GameObject.Find("LeftHandAnchor").transform.position, GameObject.Find("RightHandAnchor").transform.position, 0.5f);
            var meanHandRotation = Quaternion.Lerp(GameObject.Find("LeftHandAnchor").transform.rotation, GameObject.Find("RightHandAnchor").transform.rotation, 0.5f);

            object[] posInfoToSend = {meanHandPosition, meanHandRotation};

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
    
}
