using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

public class NetworkPlayer : MonoBehaviour, XRIDefaultInputActions.ISynchronizeActions
{

    public Transform head;
    XRIDefaultInputActions _defaultInputActions;
    
    private const byte SendIDForSync = 1;
    private PhotonView photonView;
    private int idOfPlayerToBePositioned;
    
    
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        _defaultInputActions = new XRIDefaultInputActions();
        _defaultInputActions.Synchronize.SetCallbacks(this);
        _defaultInputActions.Synchronize.Enable();
        
        if (!photonView.IsMine)
        {
            gameObject.transform.Translate(5, 0, 5);
            gameObject.transform.Rotate(0,180,0);
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            MapPosition(head, XRNode.Head);
           // MapPosition(leftHand, XRNode.LeftHand);
           // MapPosition(rightHand, XRNode.RightHand);
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
        if (photonEvent.Code == SendIDForSync)
        {
            idOfPlayerToBePositioned = (int)photonEvent.CustomData;
            Debug.Log("Player to be positioned is " + idOfPlayerToBePositioned);
        }
    }

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.position = position;
        target.rotation = rotation;
    }
    
    public void OnSendData(InputAction.CallbackContext context)
    {
        Debug.Log("My View Id is " + photonView.ViewID);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(SendIDForSync, photonView.ViewID,raiseEventOptions,SendOptions.SendReliable);
    }
}
