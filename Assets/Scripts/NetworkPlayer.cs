using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{

    public GameObject leftEye;
    public GameObject rightEye;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}