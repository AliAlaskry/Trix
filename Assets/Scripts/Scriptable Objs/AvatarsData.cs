using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Avatars Data", menuName = "Trix/Avatars", order = 1)]
public class AvatarsData : ScriptableObject, IAvatars
{
    #region Fields
    [SerializeField] List<Sprite> Avatars;

    public List<Sprite> Data { get => Avatars; }
    #endregion
}