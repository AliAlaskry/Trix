using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Frames Data", menuName = "Trix/Frames", order = 2)]
public class Frames : ScriptableObject
{
    #region Fields
    [SerializeField] List<Sprite> frames;

    public List<Sprite> Data { get => frames; }
    #endregion
}