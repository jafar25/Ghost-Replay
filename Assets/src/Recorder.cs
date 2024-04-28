using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformSnapshot
{
    public float TimeStamp;
    public Vector3 Position;
    public Quaternion Rotation;

    public TransformSnapshot(float timeStamp, Vector3 position, Quaternion rotation)
    {
        TimeStamp = timeStamp;
        Position = position;
        Rotation = rotation;
    }
}

[System.Serializable]
public class RecordedData
{
    public List<TransformSnapshot> Data = new List<TransformSnapshot>();
}

public class Recorder : MonoBehaviour
{
    /// <summary>
    /// Transform to record position and rotation for
    /// </summary>
    public Transform ObjectToRecord;
    /// <summary>
    /// Time between captures in seconds
    /// </summary>
    public float SnapshotInterval = 0.1f;
    /// <summary>
    /// Minimum distance to record position change
    /// </summary>
    public float PositionThreshold = 0.01f;
    /// <summary>
    /// Minimum angle to record rotation change (in degrees)
    /// </summary>
    public float RotationThreshold = 0.1f;
    public List<TransformSnapshot> TransformData = new List<TransformSnapshot>();
    private float startTime;
    private float timeOfLastSnapshot;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool wasStill = false;
    private bool IsRecording = false;

    public void StartRecording()
    {
        TransformData.Clear();
        IsRecording = true;
        startTime = Time.time;
        timeOfLastSnapshot = Time.time - SnapshotInterval; // Ensure first snapshot is taken immediately
        lastPosition = ObjectToRecord.position;
        lastRotation = ObjectToRecord.rotation;
        wasStill = false;

        StartCoroutine(RecordTransform());
    }

    public void StopRecording()
    {
        IsRecording = false;
        StopAllCoroutines();
    }

    private IEnumerator RecordTransform()
    {
        while (IsRecording)
        {
                float distance = Vector3.Distance(ObjectToRecord.position, lastPosition);
                float angle = Quaternion.Angle(ObjectToRecord.rotation, lastRotation);

                if (distance > PositionThreshold || angle > RotationThreshold)
                {
                    if (wasStill)
                    {
                        // Record the first frame after motion resumes
                        TransformData.Add(new TransformSnapshot(Time.time - startTime, lastPosition, lastRotation));
                        wasStill = false;
                    }
                    // Record the current frame
                    TransformData.Add(new TransformSnapshot(Time.time - startTime, ObjectToRecord.position, ObjectToRecord.rotation));
                    timeOfLastSnapshot = Time.time;
                    lastPosition = ObjectToRecord.position;
                    lastRotation = ObjectToRecord.rotation;
                }
                else if (!wasStill)
                {
                    // Record one frame when it stops moving
                    TransformData.Add(new TransformSnapshot(Time.time - startTime, ObjectToRecord.position, ObjectToRecord.rotation));
                    wasStill = true;
                }
            yield return new WaitForSeconds(SnapshotInterval);
        }
    }

    public void SaveRecording(string fileName)
    {
        RecordedData dataList = new RecordedData { Data = this.TransformData };
        string json = JsonUtility.ToJson(dataList);
        string filePath = Application.persistentDataPath + "/" + fileName;
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log("Data saved to " + filePath);
    }
}
