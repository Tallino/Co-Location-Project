using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{

    public GameObject leftEye;
    public GameObject rightEye;
    public Transform head;
    private GameObject _centerEyeAnchor;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;

            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor").gameObject;
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