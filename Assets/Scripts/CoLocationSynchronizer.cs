using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

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
    [SerializeField] 
    private TextMeshProUGUI text;

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
        if (photonEvent.Code == SendInitialPositionForCoLocation && gameObject.GetPhotonView().IsMine)
        {
            Debug.Log("Master Client " + gameObject.GetPhotonView().ViewID + " received position from client " + _idOfPlayerToBePositioned);
            object[] data = (object[]) photonEvent.CustomData;
            
            var playerToPositionMeanPointPosition = (Vector3)data[0];
            var playerToPositionMeanPointRotation = (Quaternion)data[1];
            
            Debug.Log("Received " + _idOfPlayerToBePositioned + " mean point position: " + playerToPositionMeanPointPosition);

            var masterClientRightHand = GameObject.Find("RightHandAnchor");
            var masterClientLeftHand = GameObject.Find("LeftHandAnchor");
            
            var masterClientMeanPointPosition = Vector3.Lerp(masterClientLeftHand.transform.position, masterClientRightHand.transform.position, 0.5f);
            var masterClientMeanPointRotation = Quaternion.Lerp(masterClientLeftHand.transform.rotation, masterClientRightHand.transform.rotation, 0.5f);
            masterClientMeanPointRotation.x = 0;
            masterClientMeanPointRotation.z = 0;

            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " mean point position: " + masterClientMeanPointPosition);
            
            //CALCULATING FINAL POSITION
            var finalDeltaPosition = masterClientMeanPointPosition - playerToPositionMeanPointPosition;
            finalDeltaPosition.y = 0;
            Debug.Log("DELTA POSITION TO MOVE IS: " + finalDeltaPosition);
            
            //CALCULATING FINAL ROTATION
            var finalDeltaRotation = masterClientMeanPointRotation * Quaternion.Inverse(playerToPositionMeanPointRotation);
            finalDeltaRotation.x = 0;
            finalDeltaRotation.z = 0;
            
            object[] posInfoToSend = {finalDeltaPosition, finalDeltaRotation};
            
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(SendFinalPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }

        //Event triggered at the end of the above event: sending final position to player 2 who repositions himself
        if (photonEvent.Code == SendFinalPositionForCoLocation && gameObject.GetPhotonView().IsMine && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            object[] data = (object[]) photonEvent.CustomData;
            
            var finalDeltaPosition = (Vector3)data[0];
            var finalDeltaRotation = (Quaternion)data[1];

            GameObject.Find("OVRCameraRig").transform.position += finalDeltaPosition;
            GameObject.Find("OVRCameraRig").transform.rotation *= finalDeltaRotation;

            /*
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            _idOfPlayerToBePositioned = 0;
            PhotonNetwork.RaiseEvent(ResetID, 0, raiseEventOptions, SendOptions.SendReliable);
            */
        }
        
        //Event triggered at the end of above event: sets 0 to _idOfPlayerToBePositioned on the instances of ALL the players
        if (photonEvent.Code == ResetID)
        {
            _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
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
        
        if (gameObject.GetPhotonView().IsMine && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            var tempRightHand = GameObject.Find("RightHandAnchor");
            var tempLeftHand = GameObject.Find("LeftHandAnchor");
            
            var refMeanPointPosition = Vector3.Lerp(tempLeftHand.transform.position, tempRightHand.transform.position, 0.5f);
            var refMeanPointRotation = Quaternion.Lerp(tempLeftHand.transform.rotation, tempRightHand.transform.rotation, 0.5f);
            refMeanPointRotation.x = 0;
            refMeanPointRotation.z = 0;

            object[] posInfoToSend = {refMeanPointPosition, refMeanPointRotation};
            
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent(SendInitialPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }


    private void Update()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            text.text = " " + GameObject.Find("OVRCameraRig").transform.position;
        }
    }
}
