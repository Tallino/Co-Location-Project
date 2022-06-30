using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoLocationSynchronizer : MonoBehaviourPunCallbacks, XRIDefaultInputActions.ISynchronizeActions
{
    private XRIDefaultInputActions _defaultInputActions;

    private const byte SendIDForSync = 1;
    private const byte SendInitialPositionForCoLocation = 2;
    private const byte SendFinalPositionForCoLocation = 3;
    private const byte ResetID = 4;
    private int _idOfPlayerToBePositioned;
    private const int MasterClientViewId = 1001;
    private GameObject _ovrCameraRig;

    public void Start()
    {
        _defaultInputActions = new XRIDefaultInputActions();

        if (!PhotonNetwork.IsMasterClient)
        {
            _defaultInputActions.Synchronize.SetCallbacks(this);
            _defaultInputActions.Synchronize.Enable();
        }
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
        if (photonEvent.Code == SendIDForSync)
        {
            if ((int) photonEvent.CustomData != MasterClientViewId)
            {
                _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
                Debug.Log("Player to be positioned is " + _idOfPlayerToBePositioned);
            }
        }

        //Event triggered by bunny hand gesture: received the position of the player who wants to be positioned
        if (photonEvent.Code == SendInitialPositionForCoLocation && gameObject.GetPhotonView().IsMine && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            var data = (object[]) photonEvent.CustomData;
   
            var otherRightHandPosition = (Vector3)data[0];
            var otherRightHandRotation = (Quaternion)data[1];
            
            var myRightHandRotation = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("RightHandAnchor").gameObject.transform.rotation;
            var myRightHandPosition = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("RightHandAnchor").gameObject.transform.position;

            var deltaPosition = otherRightHandPosition - myRightHandPosition;
            deltaPosition.y = 0;

            gameObject.transform.RotateAround(transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject.transform.position, Vector3.up, otherRightHandRotation.eulerAngles.y - myRightHandRotation.eulerAngles.y);
            // gameObject.transform.position += deltaPosition;
        }
    }

    public void OnSendData(InputAction.CallbackContext context)
    {
        _idOfPlayerToBePositioned = gameObject.GetPhotonView().ViewID;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
        PhotonNetwork.RaiseEvent(SendIDForSync, gameObject.GetPhotonView().ViewID, raiseEventOptions, SendOptions.SendReliable);
    }

    
    // Function triggered with hand gesture
    public void CoLocate()
    {
        if(_idOfPlayerToBePositioned == 0)
            return;
        
        Debug.Log("Co-Location initializing");

        if (gameObject.GetPhotonView().IsMine && gameObject.GetPhotonView().ViewID == MasterClientViewId)
        {
            var tempRightHand = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("RightHandAnchor").gameObject;
            
            object[] posInfoToSend = {tempRightHand.transform.position, tempRightHand.transform.rotation};

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(SendInitialPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
}
