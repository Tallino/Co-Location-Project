using Photon.Pun;

public class CustomOVRGrabber : OVRGrabber
{
    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (grabbedObject != null) 
            grabbedObject.gameObject.GetPhotonView().RequestOwnership();
    }
}
