using UnityEngine;
using Photon.Pun;
using TMPro;

public class NetworkPlayer : MonoBehaviour
{

    public GameObject leftEye;
    public GameObject rightEye;
    public GameObject meanHand;
    public GameObject canvas;
    public TextMeshProUGUI text;
    public Transform head;
    public MeshRenderer playerStateCapsule;
    
    private GameObject _centerEyeAnchor;
    private GameObject _leftHandAnchor;
    private GameObject _rightHandAnchor;

    private GameObject _leftHandTrackingPrefab;
    private GameObject _rightHandTrackingPrefab;

    private GameObject _leftHandMesh;
    private GameObject _rightHandMesh;
    
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

            _leftHandTrackingPrefab = GameObject.Find("OVRLeftHandPrefab").gameObject;
            _rightHandTrackingPrefab = GameObject.Find("OVRRightHandPrefab").gameObject;

            _leftHandMesh = GameObject.Find("hands:Lhand").gameObject;
            _rightHandMesh = GameObject.Find("hands:Rhand").gameObject;
        }
        else
        {
            canvas.SetActive(false);
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

            if (_leftHandTrackingPrefab.GetComponent<OVRHand>().IsTracked || _rightHandTrackingPrefab.GetComponent<OVRHand>().IsTracked)
            {
                _leftHandMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
                _rightHandMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
            else
            {
                _leftHandMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
                _rightHandMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
    }

    public void SetStateHasChanged(bool stateHasChanged)
    {
        _stateHasChanged = stateHasChanged;
    }
    
    private void CheckPlayerState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 1);

            text.text = "You are the MASTER CLIENT<br><br>";
            if (gameObject.GetComponent<CoLocationSynchronizer>().GetIdOfPlayerToBePositioned() == 0)
            {
                text.text += "Waiting for a player to candidate for colocation: a BLUE capsule will appear above his head";
            }
            else
            {
                text.text += "A player is ready for colocation: leave your controllers on a table near you and walk towards him (BLUE capsule above his head)<br><br>";
                text.text += "While he keeps his hands behind his back, put your open hands in front of him<br><br>";
                text.text += "When he sees BOTH of your hands, make a two sign with your right hand (close thumb, ring and pinky fingers)<br><br>";
                text.text += "Repeat until colocation is 100% accurate";
            }
        }
        else if (gameObject.GetPhotonView().ViewID == gameObject.GetComponent<CoLocationSynchronizer>().GetIdOfPlayerToBePositioned())
        {
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 2);
            text.text = "You are ready for colocation: leave your controllers on a table near you<br><br>";
            text.text += "Put your hands behind your back and walk towards the MASTER CLIENT (RED capsule above his head)";
        }
        else
        {
            gameObject.GetPhotonView().RPC("SetColor", RpcTarget.AllBuffered, 3);
            text.text = "Press Grip, Trigger and A button on your right controller to candidate for colocation";
        }

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