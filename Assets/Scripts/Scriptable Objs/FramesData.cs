using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Frames Data", menuName = "Trix/Frames", order = 2)]
public class FramesData : ScriptableObject, IFrames
{
    #region Fields
    [SerializeField] List<Sprite> Frames;

    public List<Sprite> Data { get => Frames; }
    #endregion
}