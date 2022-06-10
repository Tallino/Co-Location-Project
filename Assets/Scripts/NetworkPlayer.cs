using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    private GameObject _rig;
    private GameObject _centerEyeAnchor;
    public Transform head;
    public GameObject leftEye;
    public GameObject rightEye;

    // Start is called before the first frame update
    void Start()
    {
        _rig = GameObject.Find("OVRCameraRig");
        _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");

        
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;
        }

        if (!PhotonNetwork.IsMasterClient)
            _rig.transform.Translate(3,0,3);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            head.position = _centerEyeAnchor.transform.position;
            head.rotation = _centerEyeAnchor.transform.rotation;
        }
    }
}
