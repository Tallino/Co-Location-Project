using System;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
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
    private GameObject _ovrCameraRig;
    private GameObject _centerEyeAnchor;
    private GameObject _rightHand;
    private GameObject _leftHand;
    private Vector3 _vectorToRightHand;
    private float _rotationalAngle;
    private Vector3 _masterClientVectorToRightHand;
    private float _masterClientRotationalAngle;

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
        //Event triggered by grip button: received the id of the player who wants to be positioned
        if (photonEvent.Code == SendIDForSync && PhotonNetwork.IsMasterClient)
        {
            if ((int) photonEvent.CustomData != MasterClientViewId)
            {
                _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
                Debug.Log("Player to be positioned is " + _idOfPlayerToBePositioned);
            }
        }

        //Event triggered by bunny hand gesture: received the position of the player who wants to be positioned
        if (photonEvent.Code == SendPositionForCoLocation && gameObject.GetPhotonView().ViewID == MasterClientViewId)
        {
            Debug.Log("Master Client " + gameObject.GetPhotonView().ViewID + " received position from client " + _idOfPlayerToBePositioned);
            object[] data = (object[]) photonEvent.CustomData;
            
            var playerToPositionVectorToRightHand = (Vector3)data[0];
            var playerToPositionRotationalAngle = (float)data[1];
            
            Debug.Log("Received position: " + playerToPositionVectorToRightHand);
            Debug.Log("Received rotation: " + playerToPositionRotationalAngle);
            
            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
            _rightHand = GameObject.Find("OVRRightHandPrefab");
            _leftHand = GameObject.Find("OVRLeftHandPrefab");
            
            _masterClientVectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
            _masterClientRotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);
            
            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " position: " + _masterClientVectorToRightHand);
            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " rotation: " + _masterClientRotationalAngle);
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

        if (gameObject.GetPhotonView().IsMine)
        {
            Debug.Log("RPC CALLED BY " + gameObject.GetPhotonView());
            gameObject.GetPhotonView().RPC("SendPositionToMasterClient", RpcTarget.Others);
        }
    }

    [PunRPC]
    public void SendPositionToMasterClient()
    {
        if (gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            Debug.Log("RPC FUNCTION IS ABOUT TO GET EXECUTED BY " + gameObject.GetPhotonView());
            
            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
            _rightHand = GameObject.Find("OVRRightHandPrefab");
            _leftHand = GameObject.Find("OVRLeftHandPrefab");
            
            _vectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
            _rotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);

            Debug.Log("My view ID is: " + gameObject.GetPhotonView().ViewID);
            Debug.Log("VectorToRightHand to send to Master is : " + _vectorToRightHand);
            Debug.Log("RotationalAngle to send to Master is : " + _rotationalAngle);

            object[] posInfoToSend = {_vectorToRightHand, _rotationalAngle};
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
            Debug.Log("Sent position to Master Client");
        }
    }
}