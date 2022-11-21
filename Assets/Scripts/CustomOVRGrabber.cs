using Photon.Pun;

public class CustomOVRGrabber : OVRGrabber
{
    
    //When a player grabs a box, it becomes its owner, allowing him to replicate the box's movement to all other clients
    public override void Update()
    {
        base.Update();

        if (grabbedObject != null) 
            grabbedObject.gameObject.GetPhotonView().RequestOwnership();
    }
}
