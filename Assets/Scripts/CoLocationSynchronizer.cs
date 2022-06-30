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
            var otherVectorToRightHand = (Vector3)data[1];

            var myRightHand = GameObject.Find("RightHandAnchor");
            var myVectorToRightHand = myRightHand.transform.position - GameObject.Find("CenterEyeAnchor").transform.position;

            var deltaRotation = Vector3.Angle(otherVectorToRightHand, myVectorToRightHand);
            
            var deltaPosition = otherRightHandPosition - myRightHand.transform.position;
            deltaPosition.y = 0;

            GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(GameObject.Find("CenterEyeAnchor").gameObject.transform.position, Vector3.up, deltaRotation);
            // gameObject.transform.position += deltaPosition;
            
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(10, deltaRotation, raiseEventOptions, SendOptions.SendReliable);
        }
        
        if (photonEvent.Code == 10 && gameObject.GetPhotonView().IsMine)
            Debug.Log((float)photonEvent.CustomData);
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
            var tempRightHand = GameObject.Find("RightHandAnchor");
            var vectorToRightHand = tempRightHand.transform.position - GameObject.Find("CenterEyeAnchor").transform.position;
                                    
            object[] posInfoToSend = {tempRightHand.transform.position, vectorToRightHand};

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(SendInitialPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
}
