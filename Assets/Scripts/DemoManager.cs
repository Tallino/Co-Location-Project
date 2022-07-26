using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemoManager : MonoBehaviour, XRIDefaultInputActions.IDemo1Actions, XRIDefaultInputActions.IDemo2Actions
{
    private XRIDefaultInputActions _defaultInputActions;
    private GameObject _tempCircle;
    private GameObject _cross;
    private bool _circleIsDrawn;
    
    // Start is called before the first frame update
    void Start()
    {
        _defaultInputActions = new XRIDefaultInputActions();

        _defaultInputActions.Demo1.SetCallbacks(this);
        _defaultInputActions.Demo1.Enable();
        _defaultInputActions.Demo2.SetCallbacks(this);
        _defaultInputActions.Demo2.Enable();
    }

    public void OnSpawnCircles(InputAction.CallbackContext context)
    {
        if (gameObject.GetPhotonView().IsMine && context.started)
        {
            if (!_circleIsDrawn)
            {
                var tempPos = new Vector3(gameObject.GetComponent<NetworkPlayer>().head.position.x, 0, gameObject.GetComponent<NetworkPlayer>().head.position.z - 0.2f);
                _tempCircle = PhotonNetwork.Instantiate("Circle", tempPos, new Quaternion(0,0,0,0));
                gameObject.GetPhotonView().RPC("DrawCircle", RpcTarget.AllBuffered, _tempCircle.GetPhotonView().ViewID);
                _circleIsDrawn = true;
            }
            else
            {
                PhotonNetwork.Destroy(_tempCircle);
                _circleIsDrawn = false;
            }
        }
    }

    public void OnSpawnMeanX(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    [PunRPC]
    private void DrawCircle(int tempId)
    {
        var segments = 360;
        var line = PhotonView.Find(tempId).gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.positionCount = segments + 1;

        var pointCount = segments + 1;
        var points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * 0.25f, 0, Mathf.Cos(rad) * 0.25f);
        }

        line.SetPositions(points);
    }
}
