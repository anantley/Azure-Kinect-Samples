using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

public class RenderPointing : MonoBehaviour
{
    
    public bool Tracker1 = true;

    public GameObject cylinderPrefab;
    Dictionary<JointId, JointId> parentJointMap;

    // Start is called before the first frame update
    void Awake()
    {
        parentJointMap = new Dictionary<JointId, JointId>();
        parentJointMap[JointId.Pelvis] = JointId.Count;
        parentJointMap[JointId.SpineNavel] = JointId.Pelvis;
        parentJointMap[JointId.SpineChest] = JointId.SpineNavel;
        parentJointMap[JointId.Neck] = JointId.SpineChest;
        parentJointMap[JointId.ClavicleLeft] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderLeft] = JointId.ClavicleLeft;
        parentJointMap[JointId.ElbowLeft] = JointId.ShoulderLeft;
        parentJointMap[JointId.WristLeft] = JointId.ElbowLeft;
        parentJointMap[JointId.HandLeft] = JointId.WristLeft;
        parentJointMap[JointId.HandTipLeft] = JointId.HandLeft;
        parentJointMap[JointId.ThumbLeft] = JointId.HandLeft;
        parentJointMap[JointId.ClavicleRight] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderRight] = JointId.ClavicleRight;
        parentJointMap[JointId.ElbowRight] = JointId.ShoulderRight;
        parentJointMap[JointId.WristRight] = JointId.ElbowRight;
        parentJointMap[JointId.HandRight] = JointId.WristRight;
        parentJointMap[JointId.HandTipRight] = JointId.HandRight;
        parentJointMap[JointId.ThumbRight] = JointId.HandRight;
        parentJointMap[JointId.HipLeft] = JointId.SpineNavel;
        parentJointMap[JointId.KneeLeft] = JointId.HipLeft;
        parentJointMap[JointId.AnkleLeft] = JointId.KneeLeft;
        parentJointMap[JointId.FootLeft] = JointId.AnkleLeft;
        parentJointMap[JointId.HipRight] = JointId.SpineNavel;
        parentJointMap[JointId.KneeRight] = JointId.HipRight;
        parentJointMap[JointId.AnkleRight] = JointId.KneeRight;
        parentJointMap[JointId.FootRight] = JointId.AnkleRight;
        parentJointMap[JointId.Head] = JointId.Neck;
        parentJointMap[JointId.Nose] = JointId.Count;
        parentJointMap[JointId.EyeLeft] = JointId.Count;
        parentJointMap[JointId.EarLeft] = JointId.Count;
        parentJointMap[JointId.EyeRight] = JointId.Count;
        parentJointMap[JointId.EarRight] = JointId.Count;
    }

    private void Start()
    {
        Debug.Log("number of children of the tracker is " + transform.childCount);
        Debug.Log("number of joints of the tracker is " + transform.GetChild(0).childCount);
    }


    void Update()
    {

        if (GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms.Count > 0)
        {
            int currentJointId = 0;
            foreach (Transform pointJoint in transform.GetChild(0))
            {
                if (Tracker1 && GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms.Count > 0)
                {
                    pointJoint.transform.localPosition = GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms[(JointId)currentJointId].Item1;
                    pointJoint.transform.localRotation = GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms[(JointId)currentJointId].Item2;
                    // if there is a parent then put the bone in place
                    if (parentJointMap[(JointId)currentJointId] != JointId.Count)
                    {
                        Vector3 boneDirectionTrackerSpace4 = GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms[(JointId)currentJointId].Item1 -
                            GameObject.Find("Main").GetComponent<ReadPointData>().jointTransforms[parentJointMap[(JointId)currentJointId]].Item1;
                        Vector3 boneDirectionWorldSpace4 = transform.rotation * boneDirectionTrackerSpace4;
                        Vector3 boneDirectionLocalSpace4 = Quaternion.Inverse(pointJoint.transform.rotation) * Vector3.Normalize(boneDirectionWorldSpace4);
                        pointJoint.transform.GetChild(0).transform.localScale = new Vector3(1, 20.0f * 0.5f * boneDirectionWorldSpace4.magnitude, 1);
                        pointJoint.transform.GetChild(0).transform.localRotation = Quaternion.FromToRotation(Vector3.up, boneDirectionLocalSpace4);
                        pointJoint.transform.GetChild(0).transform.position = pointJoint.transform.position - 0.5f * boneDirectionWorldSpace4;
                    }
                }
                currentJointId++;
            }
        }
    }
}
