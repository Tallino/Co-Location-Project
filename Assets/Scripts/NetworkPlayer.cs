using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{

    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject meanHand;
    public Transform head;
    public MeshRenderer playerStateCapsule;
    
    private GameObject _centerEyeAnchor;
    private GameObject _leftHandAnchor;
    private GameObject _rightHandAnchor;
    private bool _stateHasChanged = true;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            leftEye.GetComponent<MeshRenderer>().enabled = false;
            rightEye.GetComponent<MeshRenderer>().enabled = false;

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

            meanHand.transform.position = Vector3.Lerp(_leftHandAnchor.transform.position, _rightHandAnchor.transform.position, 0.5f);
            meanHand.transform.rotation = Quaternion.Lerp(_leftHandAnchor.transform.rotation, _rightHandAnchor.transform.rotation, 0.5f);

            if (_stateHasChanged)
                CheckPlayerState();
        }
    }

    public void SetStateHasChanged(bool stateHasChanged)
    {
        _stateHasChanged = stateHasChanged;
    }


    private void CheckPlayerState()
    {
        if (PhotonNetwork.IsMasterClient)
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 1);
        else if (gameObject.GetPhotonView().ViewID == gameObject.GetComponent<CoLocationSynchronizer>().GetIdOfPlayerToBePositioned())
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 2);
        else
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 3);

        _stateHasChanged = false;
    }
    
    [PunRPC]
    private void SetColor(int colorId)
    {
        if (colorId == 1)
        {
            playerStateCapsule.enabled = true;
            playerStateCapsule.material.color = Color.red;
        }
        else if (colorId == 2)
        {
            playerStateCapsule.enabled = true;
            playerStateCapsule.material.color = Color.blue;
        }
        else
            playerStateCapsule.enabled = false;
    }
}