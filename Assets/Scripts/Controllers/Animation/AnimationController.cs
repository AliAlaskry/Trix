using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEditor.Progress;

public class AnimationController : MonoBehaviour
{
    #region Singlton
    public static AnimationController Instance;
    #endregion

    #region Fields
    [SerializeField] List<ObjToBeAnimatedItem> Items;
    #endregion

    #region Unity Fns
    private void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Items = new List<ObjToBeAnimatedItem>();
    }

    private void FixedUpdate()
    {
        RemoveGarbageItems();
        foreach (var item in Items)
        {
            if(item.Delay > 0)
            {
                item.Delay -= Time.fixedDeltaTime;
                continue;
            }

            item.Time += Time.fixedDeltaTime;

            if (item.Time > item.LastKeyTime) item.Time = item.LastKeyTime;

            float percentage = item.Speed.Evaluate(item.Time) / item.Speed.Evaluate(item.LastKeyTime);

            RectTransform rectTransform = item.GetRectTransform();
            if (item.IsRectTrans)
            {
                rectTransform.anchoredPosition3D = item.InitialPostion + (item.TargetPosition * percentage);
                rectTransform.sizeDelta = item.InitialSize + (item.TargetSize * percentage);
            }
            else
            {
                item.Trans.transform.position = item.InitialPostion + (item.TargetPosition * percentage);
                item.Trans.transform.localScale = item.InitialSize + (item.TargetSize * percentage);
            }

            rectTransform.localEulerAngles = item.InitialRotation + (item.TargetRotation * percentage);

            if (percentage == 1)
                item.AtFinish?.Invoke();
        }
    }
    #endregion

    #region Calls
    public void Animate(Transform Trans, AnimationCurve spped, Vector3 targetPos, Vector3 targetRot, Vector3 targetSize, Action atFinish, float delay = 0)
    {
        ObjToBeAnimatedItem newItem = new ObjToBeAnimatedItem(Trans, spped, targetPos, targetRot, targetSize, atFinish, delay);
        int itemIndex = Items.FindIndex(o => o.Trans == Trans);

        if (itemIndex != -1)
        {
            Items[itemIndex] = newItem;
            return;
        }

        Items.Add(newItem);
    }

    public static AnimationCurve LinearAnimation(float time)
    {
        return new AnimationCurve(new Keyframe(0, 0), new Keyframe(time, 1));
    }

    public ObjToBeAnimatedItem TryGetAnimatedItem(Transform trans)
    {
        return Items.Find(o => o.Trans == trans); 
    }

    void RemoveGarbageItems()
    {
        for(int i = 0; i < Items.Count;)
        {
            ObjToBeAnimatedItem item = Items[i];
            if (item.Trans.IsNull() || item.Time == item.LastKeyTime)
            {
                Items.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }
    #endregion
}

[Serializable]
public class ObjToBeAnimatedItem
{
    #region Constructor
    public ObjToBeAnimatedItem(Transform trans, AnimationCurve speed, Vector3 targetPos, Vector3 targetRot, Vector3 targetSize, Action atFinish, float delay = 0)
    {
        IsRectTrans = trans is RectTransform;
        Trans = trans;

        if (IsRectTrans)
        {
            InitialPostion = GetRectTransform().anchoredPosition3D;
            InitialSize = GetRectTransform().sizeDelta;
        }
        else
        {
            InitialPostion = trans.position;
            InitialSize = trans.localScale;
        }
        InitialRotation = trans.localEulerAngles;

        Speed = speed;

        Keyframe lastKey = speed[speed.length - 1];
        LastKeyTime = lastKey.time;

        TargetPosition = targetPos - InitialPostion;

        float x = Mathf.DeltaAngle(InitialRotation.x, targetRot.x), y = Mathf.DeltaAngle(InitialRotation.y, targetRot.y), z = Mathf.DeltaAngle(InitialRotation.z, targetRot.z);
        TargetRotation = new Vector3(x, y, z);
        
        TargetSize = targetSize - InitialSize;
       
        AtFinish = atFinish;

        Delay = delay;
        Time = 0;
    }
    #endregion

    #region Fields
    public readonly Transform Trans;

    public readonly AnimationCurve Speed;
    public readonly float LastKeyTime;

    public readonly Vector3 InitialPostion;
    public readonly Vector3 InitialRotation;
    public readonly Vector3 InitialSize;

    public readonly Vector3 TargetPosition;
    public readonly Vector3 TargetRotation;
    public readonly Vector3 TargetSize;

    public Action AtFinish;

    public bool IsRectTrans;

    public float Time;

    public float Delay;
    #endregion

    #region Calls
    public RectTransform GetRectTransform()
    {
        return Trans as RectTransform;
    }
    #endregion
}