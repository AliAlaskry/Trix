using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Names Data", menuName = "Trix/Names", order = 3)]
public class NamesData : ScriptableObject, INames
{
    #region Fields
    [SerializeField] List<string> Names;

    public List<string> Data { get => Names; }
    #endregion
}