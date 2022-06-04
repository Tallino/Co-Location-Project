using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoLocationSynchronizer : MonoBehaviour, XRIDefaultInputActions.ISynchronizeActions
{
    XRIDefaultInputActions _defaultInputActions;

    private const byte SendIDForSync = 1;
    private const byte SendPositionForCoLocation = 2;
    private PhotonView _photonView;
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
        _photonView = GetComponent<PhotonView>();
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
            object[] data = (object[]) photonEvent.CustomData;
            var playerToPositionVectorToRightHand = (Vector3)data[0];
            var playerToPositionRotationalAngle = (Vector3)data[1];

            playerToBePositioned = PhotonView.Find(_idOfPlayerToBePositioned).gameObject;
            
            // NOW POSITION playerToBePositioned in a point, to calculate using playerToPositionVectorToRightHand and playerToPositionRotationalAngle
        }
    }

    public void OnSendData(InputAction.CallbackContext context)
    {
        Debug.Log("My View Id is " + _photonView.ViewID);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
        PhotonNetwork.RaiseEvent(SendIDForSync, _photonView.ViewID, raiseEventOptions, SendOptions.SendReliable);
    }

    public void CoLocate()
    {
        _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        _rightHand = GameObject.Find("OculusHand_R");
        _leftHand = GameObject.Find("OculusHand_L");
        
        if (!PhotonNetwork.IsMasterClient)
        {
            _vectorToRightHand = _rightHand.transform.position - _centerEyeAnchor.transform.position;
            _rotationalAngle = Vector3.Angle(_vectorToRightHand, _leftHand.transform.position - _rightHand.transform.position);

            object[] posInfoToSend = {_vectorToRightHand, _rotationalAngle};
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}