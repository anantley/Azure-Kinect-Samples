using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Microsoft.Azure.Kinect.BodyTracking;
using jointInfo = System.Tuple<UnityEngine.Vector3, UnityEngine.Quaternion>;

public class Pose
{
    jointInfo fred = new jointInfo(Vector3.zero, Quaternion.identity);

    public const int POSE_SIZE = 26;
    public Quaternion[] poseJoints;
    public Vector3 translation;
    public bool[] dirty;
    
    

    public Pose()
    {
        poseJoints = new Quaternion[POSE_SIZE];
        translation = Vector3.zero;
        dirty = new bool[POSE_SIZE];
    }

}

public class ReadPointData : MonoBehaviour
{

    public string bodyTrackRecordingFileName;
    StreamReader inp_stm;
    //public Pose currentPose { get; set; }
    int lineCount;
    public Dictionary<JointId, jointInfo> jointTransforms;
    bool DotNetFile = false;
    public int timeScale = 1;
    int frameCount = 0;
    string objectName;


    private void Awake()
    {
        lineCount = 0;
        inp_stm = new StreamReader(bodyTrackRecordingFileName);
        jointTransforms = new Dictionary<JointId, jointInfo>();
    }

    public Quaternion stringToQuat(string s)
    {
        string[] temp = (DotNetFile) ? s.Substring(0, s.Length).Split(':','X','Y','Z'): s.Substring(0, s.Length).Split(':', 'W', 'Y', 'Z');
        // remember that w is first in these quats, not sure about unity quats.
      
         return new Quaternion(float.Parse(temp[1]), float.Parse(temp[3]), float.Parse(temp[5]), float.Parse(temp[7]));
        
        
    }

    public Vector3 stringToVector(string s)
    {
        string[] temp = (DotNetFile)? s.Substring(0, s.Length).Split('(',','): s.Substring(0, s.Length).Split('<', ',');
        return new Vector3(float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        frameCount++;
        //actually this happens every frame.
        if (!inp_stm.EndOfStream && frameCount == timeScale)
        {
            frameCount = 0;
            string inp_ln = inp_stm.ReadLine();
            if (!inp_ln.Contains("start") && !inp_ln.Contains("end"))
            {
                string[] splitLine =(DotNetFile)? inp_ln.Substring(1, inp_ln.Length - 2).Split('M', ']', ')'): inp_ln.Substring(1, inp_ln.Length - 2).Split('M', '>', '}');
                int count = 1;
                while (count < splitLine.Length - 2)
                {
                    Vector3 pos = stringToVector(splitLine[count]);
                    pos.y *= -1.0f;
                    Quaternion rot = stringToQuat(splitLine[count + 1]);
                    if (count / 2 < (int)JointId.Count)
                    {
                        if (inp_ln.Contains("T1"))
                        {
                            jointTransforms[(JointId)(count / 2)] = new jointInfo(pos, rot);
                        }
                    }
                    count += 2;
                }
                lineCount++;
                if (lineCount == 130)
                {
                    Debug.Log(jointTransforms[JointId.Pelvis].Item1 + " " + jointTransforms[JointId.Pelvis].Item2);
                }
            }
        }
        else if (frameCount == timeScale)
        {
            Debug.Log("Reached the end of the body tracking input file." + lineCount);
        }

    }

    
}
