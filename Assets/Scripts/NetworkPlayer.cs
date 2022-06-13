using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    private GameObject _rig;
    private GameObject _centerEyeAnchor;

    // Start is called before the first frame update
    void Start()
    {
        _rig = GameObject.Find("OVRCameraRig");
        _centerEyeAnchor = GameObject.Find("CenterEyeAnchor");

        if (!PhotonNetwork.IsMasterClient)
            _rig.transform.position = new Vector3(3, 0, 3);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            gameObject.transform.position = _centerEyeAnchor.transform.position;
            gameObject.transform.rotation = _centerEyeAnchor.transform.rotation;
        }
    }
}
