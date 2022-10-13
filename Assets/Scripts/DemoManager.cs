using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemoManager : MonoBehaviour, XRIDefaultInputActions.IDemo1Actions, XRIDefaultInputActions.IDemo2Actions
{
    private XRIDefaultInputActions _defaultInputActions;
    private GameObject _tempCircle;
    private GameObject _tempCross;
    private bool _circleIsDrawn;
    private bool _crossIsDrawn;

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
                var tempPos = new Vector3(gameObject.GetComponent<NetworkPlayer>().head.position.x, 0, gameObject.GetComponent<NetworkPlayer>().head.position.z);
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
        if (gameObject.GetPhotonView().IsMine && context.started && !PhotonNetwork.IsMasterClient)
        {
            if (!_crossIsDrawn)
            {
                var photonViews = FindObjectsOfType<PhotonView>();
                var masterClientId = 0;

                foreach (var view in photonViews)
                    if (view.Owner is {IsMasterClient: true})
                        masterClientId = view.ViewID;

                var masterClientPosition = PhotonView.Find(masterClientId).gameObject.GetComponent<NetworkPlayer>().head.position;
                var myPosition = gameObject.GetComponent<NetworkPlayer>().head.position;
                var meanPosition = Vector3.Lerp(masterClientPosition, myPosition, 0.5f);
                var tempPos = new Vector3(meanPosition.x, 0, meanPosition.z);
                
                _tempCross = PhotonNetwork.Instantiate("Cross", tempPos, new Quaternion(0,0,0,0));
                gameObject.GetPhotonView().RPC("DrawCross", RpcTarget.AllBuffered, _tempCross.GetPhotonView().ViewID);
                _crossIsDrawn = true;
            }
            else
            {
                PhotonNetwork.Destroy(_tempCross);
                _crossIsDrawn = false;
            }
        }
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

    [PunRPC]
    private void DrawCross(int tempId)
    {
        var segments = 360;
        var pointCount = segments + 1;
        var crossRadius = 0.25f;
        var step = crossRadius/pointCount;
        var counter = 0;

        var line = PhotonView.Find(tempId).gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.positionCount = pointCount * 4 + 4;

        var angle1 = 45 * Mathf.Deg2Rad;
        var angle2 = 135 * Mathf.Deg2Rad;
        var angle3 = 225 * Mathf.Deg2Rad;
        var angle4 = 315 * Mathf.Deg2Rad;

        for (int i = 0; i < pointCount; i++)
            line.SetPosition(counter++, new Vector3(Mathf.Sin(angle1), 0, Mathf.Cos(angle1)) * i * step);
        
        line.SetPosition(counter++, Vector3.zero);
        
        for (int i = 0; i < pointCount; i++)
            line.SetPosition(counter++, new Vector3(Mathf.Sin(angle2), 0, Mathf.Cos(angle2)) * i * step);
        
        line.SetPosition(counter++, Vector3.zero);
        
        for (int i = 0; i < pointCount; i++)
            line.SetPosition(counter++, new Vector3(Mathf.Sin(angle3), 0, Mathf.Cos(angle3)) * i * step);
        
        line.SetPosition(counter++, Vector3.zero);
        
        for (int i = 0; i < pointCount; i++)
            line.SetPosition(counter++, new Vector3(Mathf.Sin(angle4), 0, Mathf.Cos(angle4)) * i * step);
        
        line.SetPosition(counter, Vector3.zero);
    }
}
