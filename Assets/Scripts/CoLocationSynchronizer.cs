using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoLocationSynchronizer : MonoBehaviour, XRIDefaultInputActions.ISynchronizeActions
{

  XRIDefaultInputActions _defaultInputActions;
    
  private const byte SendIDForSync = 1;
  private PhotonView _photonView;
  private int _idOfPlayerToBePositioned;
  
  public void Start()
  {
      _photonView = GetComponent<PhotonView>();
      _defaultInputActions = new XRIDefaultInputActions();
      _defaultInputActions.Synchronize.SetCallbacks(this);
      _defaultInputActions.Synchronize.Enable();
  }

  private void OnEnable()
  {
      PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
  }

  private void OnDisable()
  {
      PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
  }

  private void OnEvent(EventData photonEvent)
  {
        if (photonEvent.Code == SendIDForSync)
        {
          _idOfPlayerToBePositioned = (int)photonEvent.CustomData;
          Debug.Log("Player to be positioned is " + _idOfPlayerToBePositioned);
        }
  }

  public void OnSendData(InputAction.CallbackContext context)
  {
        Debug.Log("My View Id is " + _photonView.ViewID);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(SendIDForSync, _photonView.ViewID,raiseEventOptions,SendOptions.SendReliable);
  }
  
}
