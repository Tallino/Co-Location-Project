using System;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public GameObject rig;
    public Transform head;
    public GameObject leftEye;
    public GameObject rightEye;
    private GameObject _centerEyeAnchor;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;
            GameObject myRig = Instantiate(rig, transform.position, transform.rotation);
            myRig.transform.parent = transform;
            
            _centerEyeAnchor = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;
        }
    }

    private void Update()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            head.position = _centerEyeAnchor.transform.position;
            head.rotation = _centerEyeAnchor.transform.rotation;
        }
    }

    private void OnGUI()
    {
        if(gameObject.GetPhotonView().IsMine)
            GUILayout.Label("My position is: " + _centerEyeAnchor.transform.position);
    }
}
