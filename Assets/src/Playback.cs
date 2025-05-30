﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Playback : MonoBehaviour
{
    [SerializeField]
    public Transform Ghost;
    [SerializeField]
    public UnityEvent OnPlaybackStarted;
    [SerializeField]
    public UnityEvent OnPlaybackCompleted;

    [SerializeField]
    public bool disableOnCompletion = false;

    private bool isPlaying = false;
    private float playbackTime = 0f;
    private List<TransformSnapshot> TransformData;
    private int currentIndex;

    void Update()
    {
        if (isPlaying)
        {
            playbackTime += Time.deltaTime;
            PlaybackTransformSmooth();
        }
    }

    private void PlaybackTransformSmooth()
    {
        if (currentIndex < TransformData.Count - 2)
        {
            if (playbackTime >= TransformData[currentIndex + 1].TimeStamp)
            {
                currentIndex++; // Move to the next segment
            }

            TransformSnapshot current = TransformData[currentIndex];
            TransformSnapshot next = TransformData[currentIndex + 1];
            float totalTime = next.TimeStamp - current.TimeStamp;
            float lerpFactor = (playbackTime - current.TimeStamp) / totalTime;

            Ghost.position = Vector3.Lerp(current.Position, next.Position, lerpFactor);
            Ghost.rotation = Quaternion.Slerp(current.Rotation, next.Rotation, lerpFactor);
        }
        else
        {
            // Stop playback after the end is reached
            this.StopPlayback();
        }
    }

    public void StartPlayback()
    {
        if (TransformData == null || TransformData.Count == 0)
        {
            Debug.LogError("No recorded data to play back");
            return;
        }

        OnPlaybackStarted?.Invoke();

        if (disableOnCompletion && !Ghost.gameObject.activeSelf)
        {
            Ghost.gameObject.SetActive(true);
        }

        isPlaying = true;
        playbackTime = 0f;
        currentIndex = 0;
        Ghost.position = TransformData[0].Position;
        Ghost.rotation = TransformData[0].Rotation;
    }

    public void StopPlayback()
    {
        isPlaying = false;
        OnPlaybackCompleted?.Invoke();
        if (disableOnCompletion)
        {
            Ghost.gameObject.SetActive(false);
        }
    }

    public void LoadFile(string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName;
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        string json = System.IO.File.ReadAllText(filePath);
        RecordedData loadedData = JsonUtility.FromJson<RecordedData>(json);
        TransformData = loadedData.Data;
        Debug.Log("Data loaded from " + filePath);
        Debug.Log(TransformData);
    }
}
