using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{

    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject meanHand;
    public Transform head;
    private GameObject _centerEyeAnchor;
    private GameObject _leftHandAnchor;
    private GameObject _rightHandAnchor;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;
            leftHand.GetComponent<MeshRenderer>().enabled = false;
            rightHand.GetComponent<MeshRenderer>().enabled = false;

            _centerEyeAnchor = GameObject.Find("CenterEyeAnchor").gameObject;
            _leftHandAnchor = GameObject.Find("LeftHandAnchor").gameObject;
            _rightHandAnchor = GameObject.Find("RightHandAnchor").gameObject;
        }
    }

    private void Update()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            head.position = _centerEyeAnchor.transform.position;
            head.rotation = _centerEyeAnchor.transform.rotation;
            
            leftHand.transform.position = _leftHandAnchor.transform.position;
            leftHand.transform.rotation = _leftHandAnchor.transform.rotation;
            
            rightHand.transform.position = _rightHandAnchor.transform.position;
            rightHand.transform.rotation = _rightHandAnchor.transform.rotation;

            meanHand.transform.position = Vector3.Lerp(leftHand.transform.position, rightHand.transform.position, 0.5f);
            meanHand.transform.rotation = Quaternion.Lerp(leftHand.transform.rotation, rightHand.transform.rotation, 0.5f);
        }
    }
}