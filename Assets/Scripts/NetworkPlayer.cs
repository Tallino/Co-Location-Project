using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

public class NetworkPlayer : MonoBehaviour
{

    public Transform head;
    // public InputActionReference sendDataReference;
    
    private PhotonView photonView;
    
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        // sendDataReference.action.started += SendData;
        
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

    void SendData(InputAction.CallbackContext context)
    {
        Debug.Log("ammerdaaaaaaaaaaaa");
    }
    

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.position = position;
        target.rotation = rotation;
    }
}
