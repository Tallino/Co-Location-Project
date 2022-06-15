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
        if (photonEvent.Code == SendPositionForCoLocation)
        {
            Vector3 position = (Vector3) photonEvent.CustomData;
            
            Debug.Log("POSITION IS: " + position);
            
            /*
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
            */
        }
    }

    public void OnSendData(InputAction.CallbackContext context)
    {
        Debug.Log("My View Id is " + gameObject.GetPhotonView().ViewID);
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
        Debug.Log("Hands can be seen by " + gameObject.GetPhotonView());
        
        if (gameObject.GetPhotonView().IsMine)
        {
            _centerEyeAnchor = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;
                
            // Sending position to master client
            PhotonView.Find(_idOfPlayerToBePositioned).RPC("SendPositionToMasterClient", RpcTarget.All, _centerEyeAnchor.transform.position);
            
            /*
            _vectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
            _rotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);

            object[] posInfoToSend = {_vectorToRightHand, _rotationalAngle};
            
            Debug.Log(_rig.transform.position);
            PhotonView.Find(_idOfPlayerToBePositioned).RPC("SendPositionToMasterClient", RpcTarget.MasterClient, posInfoToSend as object);
            */
        }
    }
    

    [PunRPC]
    public void SendPositionToMasterClient(Vector3 position)
    {
        Debug.Log("RPC RECEIVED");
        Debug.Log("POSITION: " + position);
        
        /*
        var playerToPositionVectorToRightHand = (Vector3)posInfoToSend[0];
        var playerToPositionRotationalAngle = (float)posInfoToSend[1];
        
        Debug.Log("Received position: " + playerToPositionVectorToRightHand);
        Debug.Log("Received rotation: " + playerToPositionRotationalAngle);
        Debug.Log("Received magnitude: " + playerToPositionVectorToRightHand.magnitude);

        _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        _rightHand = GameObject.Find("OVRRightHandPrefab");
        _leftHand = GameObject.Find("OVRLeftHandPrefab");
            
        _masterClientVectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
        _masterClientRotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);
        
        Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " position: " + _masterClientVectorToRightHand);
        Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " rotation: " + _masterClientRotationalAngle);
        Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " magnitude: " + _masterClientVectorToRightHand.magnitude); 
        */

    }
}
