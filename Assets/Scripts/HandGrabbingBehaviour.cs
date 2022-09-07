using Photon.Pun;
using UnityEngine;

public class HandGrabbingBehaviour : OVRGrabber
{

    private OVRHand _hand;
    private float pinchThreshold = 0.7f;
    private Vector3 _lastPos;
    private Quaternion _lastRot;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _hand = GetComponent<OVRHand>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        CheckIndexPinch();
        
        _lastPos = transform.position;
        _lastRot = transform.rotation;
    }

    private void CheckIndexPinch()
    {
        var pinchStrength = _hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        if (!m_grabbedObj && pinchStrength > pinchThreshold && m_grabCandidates.Count > 0)
        {
            GrabBegin();
            m_grabbedObj.gameObject.GetComponent<PhotonView>().RequestOwnership();
        }
        else if(m_grabbedObj && !(pinchStrength > pinchThreshold))
            GrabEnd();
    }

    protected override void GrabEnd()
    {
        if (m_grabbedObj)
        {
            var linearVelocity = (transform.position - _lastPos) / Time.fixedDeltaTime;
            var angularVelocity = (transform.eulerAngles - _lastRot.eulerAngles) / Time.fixedDeltaTime;

            GrabbableRelease(linearVelocity, angularVelocity);
        }
        
        GrabVolumeEnable(true);
    }
}
