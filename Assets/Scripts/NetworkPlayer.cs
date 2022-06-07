using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    private PhotonView photonView;
    private GameObject Rig;
    private GameObject CenterEyeAnchor;
    public Transform head;
    
    
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        Rig = GameObject.Find("OVRCameraRig");
        CenterEyeAnchor = GameObject.Find("CenterEyeAnchor");

        if (!PhotonNetwork.IsMasterClient)
        {
            Rig.transform.Translate(3,0,3);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            head.position = CenterEyeAnchor.transform.position;
            head.rotation = CenterEyeAnchor.transform.rotation;
        }
    }
}
