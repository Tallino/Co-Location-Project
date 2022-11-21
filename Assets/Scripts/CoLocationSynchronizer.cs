using System;
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
    private bool _coLocationDone;
    private bool _candidateDone;
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

    
    
    
    //Triggered by A, Trigger and Grip button simultaneously pressed on right controller from player who wants to be positioned
    //Sends his ID to master client (CODE TO SEND = SENDIDFORSYNC)
    public void OnSendData(InputAction.CallbackContext context)
    {
        if (!PhotonNetwork.IsMasterClient && gameObject.GetPhotonView().IsMine)
        {
            if (!_candidateDone)
            {
                _candidateDone = true;
                _idOfPlayerToBePositioned = gameObject.GetPhotonView().ViewID;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                PhotonNetwork.RaiseEvent(SendIDForSync, gameObject.GetPhotonView().ViewID, raiseEventOptions, SendOptions.SendReliable);
            }
            else
            {
                _candidateDone = false;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                PhotonNetwork.RaiseEvent(ResetID, 0, raiseEventOptions, SendOptions.SendReliable);
            }
        }
    }
    
    // Function triggered with "number 2" hand gesture with right hand
    public void CoLocate()
    {
        if(_idOfPlayerToBePositioned == 0 || !GameObject.Find("OVRLeftHandPrefab").gameObject.GetComponent<OVRHand>().IsTracked || !GameObject.Find("OVRRightHandPrefab").gameObject.GetComponent<OVRHand>().IsTracked)
            return;

        if (!_coLocationDone)
        {
            _coLocationDone = true;

            //Sending master client origin system information to the player who wants to be positioned (CODE TO SEND = SENDPOSITIONFORCOLOCATION)
            if (PhotonNetwork.IsMasterClient && gameObject.GetPhotonView().IsMine)
            {
                var meanHandPosition = Vector3.Lerp(GameObject.Find("LeftHandAnchor").transform.position, GameObject.Find("RightHandAnchor").transform.position, 0.5f);
                var meanHandRotation = Quaternion.Lerp(GameObject.Find("LeftHandAnchor").transform.rotation, GameObject.Find("RightHandAnchor").transform.rotation, 0.5f);
                var myForwardVector = Vector3.ProjectOnPlane(gameObject.GetComponent<NetworkPlayer>().head.forward, Vector3.up);
                    
                object[] posInfoToSend = {meanHandPosition, meanHandRotation, myForwardVector};

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                PhotonNetwork.RaiseEvent(SendPositionForCoLocation, posInfoToSend, raiseEventOptions, SendOptions.SendReliable);
            }
        }
    }
    
    //Unity RaiseEvent() receiving end
    private void OnEvent(EventData photonEvent)
    {
        // Received the id of the player who wants to be positioned (CODE RECEIVED = SENDIDFORSYNC)
        if (photonEvent.Code == SendIDForSync && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = (int) photonEvent.CustomData;
            _coLocationDone = false;
            gameObject.GetComponent<NetworkPlayer>().SetStateHasChanged(true);
        }
        
        //COLOCATION CALCULATIONS HAPPEN HERE
        //Event received after bunny hand gesture in Colocate() function: received the information about the origin system of the master client (CODE RECEIVED = SENDPOSITIONFORCOLOCATION)
        if (photonEvent.Code == SendPositionForCoLocation && gameObject.GetPhotonView().ViewID == _idOfPlayerToBePositioned && gameObject.GetPhotonView().IsMine)
        {
            var data = (object[]) photonEvent.CustomData;
   
            //Receiving Mean Hand information and Forward Vector information from the master client
            var otherMeanHandPosition = (Vector3)data[0];
            var otherMeanHandRotation = (Quaternion)data[1];
            var otherForwardVector = (Vector3)data[2];
            
            //Finding Mean Hand information of the player to be positioned (ourselves)
            var myMeanHandPosition = Vector3.Lerp(GameObject.Find("LeftHandAnchor").transform.position, GameObject.Find("RightHandAnchor").transform.position, 0.5f);
            var myMeanHandRotation = Quaternion.Lerp(GameObject.Find("LeftHandAnchor").transform.rotation, GameObject.Find("RightHandAnchor").transform.rotation, 0.5f);

            //CALCULATING ROTATION
            var deltaRotation = otherMeanHandRotation.eulerAngles.y - myMeanHandRotation.eulerAngles.y;

            //CALCULATING POSITION
            var deltaPosition = otherMeanHandPosition - myMeanHandPosition;
            deltaPosition.y = 0;

            //APPLYING ROTATION
            GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(myMeanHandPosition, Vector3.up, deltaRotation);
            gameObject.GetComponent<NetworkPlayer>().head.RotateAround(myMeanHandPosition, Vector3.up, deltaRotation);

            //GETTING MY FORWARD VECTOR
            var myForwardVector = Vector3.ProjectOnPlane(gameObject.GetComponent<NetworkPlayer>().head.forward, Vector3.up);

            //EVENTUAL EXTRA ROTATION
            if (Vector3.Dot(otherForwardVector, myForwardVector) > 0)
            {
                GameObject.Find("OVRCameraRig").gameObject.transform.RotateAround(myMeanHandPosition, Vector3.up, 180);
                gameObject.GetComponent<NetworkPlayer>().head.RotateAround(myMeanHandPosition, Vector3.up, 180);
            }

            //APPLYING POSITION
            GameObject.Find("OVRCameraRig").gameObject.transform.position += deltaPosition;

            //Resetting ID of player to be positioned (CODE TO SEND = RESETID)
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(ResetID, 0, raiseEventOptions, SendOptions.SendReliable);
        }

        //Reset ID of player to be positioned and notify state has changed (CODE RECEIVED = RESETID)
        if (photonEvent.Code == ResetID && gameObject.GetPhotonView().IsMine)
        {
            _idOfPlayerToBePositioned = (int)photonEvent.CustomData;
            gameObject.GetComponent<NetworkPlayer>().SetStateHasChanged(true);
        }
    }

    public int GetIdOfPlayerToBePositioned()
    {
        return _idOfPlayerToBePositioned;
    }
} 
