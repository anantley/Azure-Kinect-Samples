using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using System.IO;
using System;
public class main : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private BackgroundDataProvider m_backgroundDataProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    bool enableCapture = false;
    public bool replayCapture = false;

    System.IO.StreamWriter file;

    void Start()
    {
        SkeletalTrackingProvider m_skeletalTrackingProvider = new SkeletalTrackingProvider();

        //tracker ids needed for when there are two trackers
        const int TRACKER_ID = 0;
        m_skeletalTrackingProvider.StartClientThread(TRACKER_ID);
        m_backgroundDataProvider = m_skeletalTrackingProvider;

        file = new System.IO.StreamWriter(@"..\SkeletonPointCapture" + System.DateTime.Now.ToUniversalTime().Month +
                System.DateTime.Now.ToUniversalTime().Day + System.DateTime.Now.ToUniversalTime().Year + System.DateTime.Now.ToUniversalTime().Hour +
                System.DateTime.Now.ToUniversalTime().Minute + System.DateTime.Now.ToUniversalTime().Second + ".txt", false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            enableCapture = !enableCapture;
            if (enableCapture)
            {
                file.WriteLine("Begin Capture");
            }
            else
            {
                file.WriteLine("End Capture");
            }
        }

        if (!replayCapture)
        {
            if (m_backgroundDataProvider.IsRunning)
            {
                if (m_backgroundDataProvider.GetCurrentFrameData(ref m_lastFrameData))
                {
                    if (m_lastFrameData.NumOfBodies != 0)
                    {
                        m_tracker.GetComponent<TrackerHandler>().updateTracker(m_lastFrameData);
                    }
                }
            }
        }
    }


    private void FixedUpdate()
    {
        if (enableCapture && !replayCapture)
        {

            string frameJoints = "";
            frameJoints += DateTime.Now + " ";

            if (m_lastFrameData.NumOfBodies != 0)
            {
                int[] closestBodies = new int[2];
                findClosestTrackedBody(m_lastFrameData, closestBodies);
                var skeleton = m_lastFrameData.Bodies[closestBodies[0]];
                var bodyId = m_lastFrameData.Bodies[closestBodies[0]].Id;

                for (int jointId = 0; jointId < (int)JointId.Count; ++jointId)
                {
                    var jointPosition = skeleton.JointPositions3D[jointId];
                    var jointRotation = skeleton.JointRotations[jointId];
                    frameJoints += jointPosition.ToString("F4") + " " + jointRotation + " ";
                }
            }
            file.WriteLine("T1" + frameJoints);
            file.Flush();


        }
    }

    void findClosestTrackedBody(BackgroundData frameData, int[] bodies)
    {
        float[] minDistanceFromKinect = { 5000.0f, 5000.0f };
        bodies[0] = -1;
        bodies[1] = -1;

        for (int i = 0; i < (int)frameData.NumOfBodies; i++)
        {
            var pelvisPosition = frameData.Bodies[i].JointPositions3D[(int)JointId.Pelvis];
            Vector3 pelvisPos = new Vector3((float)pelvisPosition.X, (float)pelvisPosition.Y, (float)pelvisPosition.Z);
            if (pelvisPos.magnitude < minDistanceFromKinect[0])
            {
                bodies[1] = bodies[0];
                minDistanceFromKinect[1] = minDistanceFromKinect[0];
                bodies[0] = i;
                minDistanceFromKinect[0] = pelvisPos.magnitude;
            }
            else if (pelvisPos.magnitude < minDistanceFromKinect[1])
            {
                bodies[1] = i;
                minDistanceFromKinect[1] = pelvisPos.magnitude;
            }
        }
    }



    void OnDestroy()
    {
        // Stop background threads.
        if (m_backgroundDataProvider != null)
        {
            m_backgroundDataProvider.StopClientThread();
        }
    }
    
}
