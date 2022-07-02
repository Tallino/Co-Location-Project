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
            Debug.Log("Player to be positioned is " + _idOfPlayerToBePositioned);
        }

        //Event triggered by bunny hand gesture: received the information about the origin system of the master client
        if (photonEvent.Code == SendPositionForCoLocation && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned && gameObject.GetPhotonView().IsMine)
        {
            var data = (object[]) photonEvent.CustomData;
   
            var otherMeanHandPosition = (Vector3)data[0];
            var otherMeanHandRotation = (Quaternion)data[1];
            var otherHeadForward = (Vector3)data[2];
            
            var myRightHand = GameObject.Find("RightHandAnchor");
            var myLeftHand = GameObject.Find("LeftHandAnchor");
            
            var myMeanHandPosition = Vector3.Lerp(myLeftHand.transform.position, myRightHand.transform.position, 0.5f);
            var myMeanHandRotation = Quaternion.Lerp(myLeftHand.transform.rotation, myRightHand.transform.rotation, 0.5f);

            var deltaRotation = otherMeanHandRotation.eulerAngles.y - myMeanHandRotation.eulerAngles.y;

            var deltaPosition = otherMeanHandPosition - myMeanHandPosition;
            deltaPosition.y = 0;
            
            GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(myMeanHandPosition, Vector3.up, deltaRotation);
            var myHeadForward = gameObject.GetComponent<NetworkPlayer>().head.forward;
            
            if(Vector3.Dot(myHeadForward, otherHeadForward) > 0)
                GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(myMeanHandPosition, Vector3.up, 180);

            GameObject.Find("OVRCameraRig").gameObject.transform.position += deltaPosition;
            
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(ResetID, 0, raiseEventOptions, SendOptions.SendReliable);
        }

        if (photonEvent.Code == ResetID && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = (int)photonEvent.CustomData;
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
        
        Debug.Log("Co-Location initializing");

        //Sending master client origin system information to the player who wants to be positioned
        if (PhotonNetwork.IsMasterClient && gameObject.GetPhotonView().IsMine)
        {
            var tempRightHand = GameObject.Find("RightHandAnchor");
            var tempLeftHand = GameObject.Find("LeftHandAnchor");

            var meanHandPosition = Vector3.Lerp(tempLeftHand.transform.position, tempRightHand.transform.position, 0.5f);
            var meanHandRotation = Quaternion.Lerp(tempLeftHand.transform.rotation, tempRightHand.transform.rotation, 0.5f);
            
            Debug.Log("MY MEAN ROTATION IS: " + meanHandRotation.eulerAngles.y);
            
            object[] posInfoToSend = {meanHandPosition, meanHandRotation, gameObject.GetComponent<NetworkPlayer>().head.forward};

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
    
}
