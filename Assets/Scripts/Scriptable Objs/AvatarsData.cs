using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Avatars Data", menuName = "Trix/Avatars", order = 1)]
public class Avatars : ScriptableObject
{
    #region Fields
    [SerializeField] List<Sprite> avatars;

    public List<Sprite> Data { get => avatars; }
    #endregion
}