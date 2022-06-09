using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}

public class GestureDetector : MonoBehaviour, XRIDefaultInputActions.IGestureDetectionActions
{
    XRIDefaultInputActions _defaultInputActions;

    public float threshold = 0.1f;
    public bool debugMode = true;
    public List<Gesture> gestures;
    private OVRSkeleton _skeleton;
    private List<OVRBone> _fingerBones;
    private Gesture previousGesture;

    // Start is called before the first frame update
    void Start()
    {
        _skeleton = GameObject.Find("OVRRightHandPrefab").GetComponent<OVRSkeleton>();
        _defaultInputActions = new XRIDefaultInputActions();
        previousGesture = new Gesture();
        
        StartCoroutine(DelayInitBones());

        if (PhotonNetwork.IsMasterClient)
        {
            _defaultInputActions.GestureDetection.SetCallbacks(this);
            _defaultInputActions.GestureDetection.Enable();
        }

    }

    public void Update()
    {
        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());
        
        //Check if new gesture
        if (hasRecognized && !currentGesture.Equals(previousGesture))
        {
            Debug.Log("New Gesture Found: " + currentGesture.name);
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
        }
    }

    public IEnumerator DelayInitBones()
    {
        while (!_skeleton.IsInitialized)
            yield return null;

        _fingerBones = new List<OVRBone>(_skeleton.Bones);
    }

    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();

        foreach (var bone in _fingerBones)
        {
            // finger position relative to root
            data.Add(_skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        g.fingerDatas = data;
        gestures.Add(g);
    }

    public void OnSaveGesture(InputAction.CallbackContext context)
    {
        if (debugMode)
            Save();
    }

    Gesture Recognize()
    {
        Gesture currentgesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            
            for (int i = 0; i < _fingerBones.Count; i++)
            {
                Vector3 currentData = _skeleton.transform.InverseTransformPoint(_fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }

                sumDistance += distance;
            }

            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentgesture = gesture;
            }
        }

        return currentgesture;
    }
}
