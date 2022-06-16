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
            Debug.Log("Master Client " + gameObject.GetPhotonView().ViewID + " received position from client " + _idOfPlayerToBePositioned);
            object[] data = (object[]) photonEvent.CustomData;
            
            var tempPlayerToPositionVectorToRightHand = (Vector3)data[0];
            var tempPlayerToPositionRotationalAngle = (float)data[1];
            
            Debug.Log("Received " + _idOfPlayerToBePositioned + " vector to right hand: " + tempPlayerToPositionVectorToRightHand);
            Debug.Log("Received " + _idOfPlayerToBePositioned + " distance to right hand: " + tempPlayerToPositionVectorToRightHand.magnitude);
            Debug.Log("Received " + _idOfPlayerToBePositioned + " rotation angle respect to hands: " + tempPlayerToPositionRotationalAngle);
            
            var tempMasterClientCenterEyeAnchor = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;
            var tempMasterClientRightHand = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("RightHandAnchor").gameObject;
            var tempMasterClientLeftHand = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("LeftHandAnchor").gameObject;

            var tempMasterClientVectorToRightHand = tempMasterClientRightHand.transform.position - tempMasterClientCenterEyeAnchor.transform.position;
            var tempMasterClientRotationalAngle = Vector3.Angle(tempMasterClientVectorToRightHand, tempMasterClientLeftHand.transform.position - tempMasterClientRightHand.transform.position);

            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " vector to right hand: " + tempMasterClientVectorToRightHand);
            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " distance to right hand: " + tempMasterClientVectorToRightHand.magnitude);
            Debug.Log("Master client " + gameObject.GetPhotonView().ViewID + " rotation angle respect to hands: " + tempMasterClientRotationalAngle);
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
        
        if (gameObject.GetPhotonView().IsMine && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned)
        {
            var tempCenterEyeAnchor = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;
            var tempRightHand = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("RightHandAnchor").gameObject;
            var tempLeftHand = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("LeftHandAnchor").gameObject;

            var tempVectorToRightHand = tempRightHand.transform.position - tempCenterEyeAnchor.transform.position;
            var tempRotationalAngle = Vector3.Angle(tempVectorToRightHand, tempLeftHand.transform.position - tempRightHand.transform.position);

            object[] posInfoToSend = {tempVectorToRightHand, tempRotationalAngle};
            
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
}
