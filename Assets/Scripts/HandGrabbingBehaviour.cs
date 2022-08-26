using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabbingBehaviour : OVRGrabber
{

    private OVRHand _hand;
    private float pinchThreshold = 0.7f;
    private Vector3 lastPos;
    private Quaternion lastRot;
    
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
        
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void CheckIndexPinch()
    {
        float pinchStrength = _hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        
        if(!m_grabbedObj && pinchStrength > pinchThreshold && m_grabCandidates.Count > 0)
            GrabBegin();
        else if(m_grabbedObj && !(pinchStrength > pinchThreshold))
            GrabEnd();
    }

    protected override void GrabEnd()
    {
        if (m_grabbedObj)
        {

            Vector3 linearVelocity = (transform.position - lastPos) / Time.fixedDeltaTime;
            Vector3 angularVelocity = (transform.eulerAngles - lastRot.eulerAngles) / Time.fixedDeltaTime;

            Debug.Log("linearVelocity: " + linearVelocity);
            Debug.Log("angularVelocity: " + angularVelocity);
            
            GrabbableRelease(linearVelocity, angularVelocity);
        }
        
        GrabVolumeEnable(true);
    }
}
