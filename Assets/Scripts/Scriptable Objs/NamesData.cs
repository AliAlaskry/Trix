using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Names Data", menuName = "Trix/Names", order = 3)]
public class Names : ScriptableObject
{
    #region Fields
    [SerializeField] List<string> names;

    public List<string> Data { get => names; }
    #endregion
}