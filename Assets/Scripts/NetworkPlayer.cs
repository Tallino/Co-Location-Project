using UnityEngine;
using Photon.Pun;
using UnityEngine.XR;

public class NetworkPlayer : MonoBehaviour
{
    private PhotonView photonView;
    private GameObject Rig;
    public GameObject head;
    
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        Rig = GameObject.Find("OVRCameraRig");
        Rig.transform.position = new Vector3(3, 0, 3);
        //head.transform.position = new Vector3(2, 0, 2);
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            MapPosition(head.transform, XRNode.Head);
           // MapPosition(leftHand, XRNode.LeftHand);
           // MapPosition(rightHand, XRNode.RightHand);
        }
        Debug.Log(head.gameObject.transform.position);
    }
  
    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.position = position;
        target.rotation = rotation;
    }
}
