using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoLocationSynchronizer : MonoBehaviour, XRIDefaultInputActions.ISynchronizeActions
{
    private XRIDefaultInputActions _defaultInputActions;

    private const byte SendIDForSync = 1;
    private const byte SendPositionForCoLocation = 2;
    private int _idOfPlayerToBePositioned;
    private const int MasterClientViewId = 1001;
    private GameObject _centerEyeAnchor;
    private GameObject _rightHand;
    private GameObject _leftHand;
    private GameObject playerToBePositioned;
    private Vector3 _vectorToRightHand;
    private float _rotationalAngle;
    


    public void Start()
    {
        _defaultInputActions = new XRIDefaultInputActions();
            
        if (!PhotonNetwork.IsMasterClient)
        {
            _defaultInputActions.Synchronize.SetCallbacks(this);
            _defaultInputActions.Synchronize.Enable();
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SendIDForSync && PhotonNetwork.IsMasterClient)
        {
            if ((int) photonEvent.CustomData != MasterClientViewId)
            {
                _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
                Debug.Log("Player to be positioned is " + _idOfPlayerToBePositioned);
            }
        }

        if (photonEvent.Code == SendPositionForCoLocation && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client received the position");
            object[] data = (object[]) photonEvent.CustomData;
            
            var playerToPositionVectorToRightHand = (Vector3)data[0];
            var playerToPositionRotationalAngle = (float)data[1];
            
            Debug.Log("Received position: " + playerToPositionVectorToRightHand);
            Debug.Log("Received rotation: " + playerToPositionRotationalAngle);
            
            playerToBePositioned = PhotonView.Find(_idOfPlayerToBePositioned).gameObject;

            // NOW POSITION playerToBePositioned in a point, to calculate using playerToPositionVectorToRightHand and playerToPositionRotationalAngle
        }
    }

    public void OnSendData(InputAction.CallbackContext context)
    {
        Debug.Log("My View Id is " + gameObject.GetPhotonView().ViewID);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
        PhotonNetwork.RaiseEvent(SendIDForSync, gameObject.GetPhotonView().ViewID, raiseEventOptions, SendOptions.SendReliable);
    }

    public void CoLocate()
    {
        Debug.Log("Co-Location initializing");
        Debug.Log("Hands can be seen by " + gameObject.GetPhotonView());
        
        if (gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
            _rightHand = GameObject.Find("OVRRightHandPrefab");
            _leftHand = GameObject.Find("OVRLeftHandPrefab");
    
            _vectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
            _rotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);
            
            Debug.Log("VectorToRightHand to send to Master is : " + _vectorToRightHand);
            Debug.Log("RotationalAngle to send to Master is : " + _rotationalAngle);
            
            object[] posInfoToSend = {_vectorToRightHand, _rotationalAngle};
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
            Debug.Log("Sent position to Master Client");
        }
    }
}