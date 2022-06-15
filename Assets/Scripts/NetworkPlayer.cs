using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    public GameObject rig;
    private GameObject _centerEyeAnchor;
    public Transform head;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            GameObject myRig = Instantiate(rig, transform.position, transform.rotation);
            myRig.transform.parent = transform;

            _centerEyeAnchor = transform.Find("OVRCameraRig(Clone)").Find("TrackingSpace").Find("CenterEyeAnchor").gameObject;

            if (!PhotonNetwork.IsMasterClient)
                myRig.transform.position = new Vector3(5, 0, 5);
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
}
