using LocalStore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public interface IDewaniaPlayer : IEqualityComparer<DewaniaPlayer>, ICloneable, IEquatable<DewaniaPlayer>
{
    bool IsBot { get; }
    string ID { get; }
    bool IsLocal { get; }
    string Name { get; }
    string Pic { get; }
    int Level { get; }
    int Points { get; }
}

public enum ConnectionStateEnum { Online, Offline }

[Serializable]
public class DewaniaPlayer : IDewaniaPlayer
{
    #region Constructor
    public DewaniaPlayer(bool _isBot, string _id, string _name, string _pic, string frame, int _level, int _points, bool isOnline)
    {
        isbot = _isBot;
        id = _id;
        name = _name;

        pic = _pic;
        GetAvatar();
        
        frameIndex = frame;
        SetRandomFrame();

        level = _level;
        points = _points;

        UpdateConnectionState(isOnline);

        if (_id == DewaniaSession.LocalPlayerId)
            isLocal = true;
        else
            isLocal = false;
    }
    #endregion

    #region Fns
    void GetAvatar()
    {
        if (!Uri.IsWellFormedUriString(pic, UriKind.Absolute))
        {
            SetRandomAvatar();
            return;
        }

        NetworkInstance.Instance.StartCoroutine(DownlaodTexture.Download(pic, (avatar) =>
        {
            Avatar = Sprite.Create(avatar, new Rect(0, 0, avatar.width, avatar.height), Vector2.zero);
        }, () =>
        {
            SetRandomAvatar();
        }));
    }

    void SetRandomAvatar()
    {
        if (int.TryParse(pic, out _)) return;

        pic = NetworkInstance.Instance.Constants.GetRandomAvatarIndex().ToString();

        avatar = NetworkInstance.Instance.Constants.GetAvatar(int.Parse(pic));
    }

    void SetRandomFrame()
    {
        if (int.TryParse(frameIndex, out _)) return;

        frameIndex = NetworkInstance.Instance.Constants.GetRandomFrameIndex().ToString();
   
        frame = NetworkInstance.Instance.Constants.GetFrame(int.Parse(frameIndex));
    }

    public bool IsOnline()
    {
        return ConnectionState == "Online";
    }

    public void UpdateConnectionState(bool isOnline)
    {
        ConnectionState = isOnline ? ConnectionStateEnum.Online.ToString() : ConnectionStateEnum.Offline.ToString();
    }

    public bool Equals(DewaniaPlayer x, DewaniaPlayer y)
    {
        if (x.id == y.id) return true;
        return false;
    }

    public int GetHashCode(DewaniaPlayer obj)
    {
        return obj.id.GetHashCode();
    }

    public object Clone()
    {
        DewaniaPlayer p = new DewaniaPlayer(isbot, id, name, pic, frameIndex, level, points, IsOnline());
        return p;
    }

    public bool Equals(DewaniaPlayer other)
    {
        if (id == other.id) return true;
        return false;
    }
    #endregion

    #region Fields

    #region Json
    [JsonProperty("isbot")]
    [SerializeField] private bool isbot;

    [JsonProperty("id")]
    [SerializeField] private string id;

    [JsonProperty("isLocal")]
    [SerializeField] private bool isLocal;

    [JsonProperty("name")]
    [SerializeField] private string name;

    [JsonProperty("picture")]
    [SerializeField] private string pic;

    [JsonProperty("level")]
    [SerializeField] private int level;

    [JsonProperty("points")]
    [SerializeField] private int points;

    [JsonProperty("connectionState")]
    [SerializeField] private string ConnectionState;
    #endregion

    [JsonIgnore]
    string frameIndex;
    
    [JsonIgnore]
    Sprite avatar;

    [JsonIgnore]
    Sprite frame;

    [JsonIgnore]
    public List<Image> AvatarImages = new List<Image>();

    [JsonIgnore]
    public List<Image> FrameImages = new List<Image>();
    #endregion

    #region Props
    [JsonIgnore]
    public bool IsBot
    {
        get { return isbot; }
    }

    [JsonIgnore]
    public string ID
    {
        get { return id; }
    }

    [JsonIgnore]
    public bool IsLocal
    {
        get { return isLocal; }
    }

    [JsonIgnore]
    public string Name
    {
        get { return name; }
    }

    [JsonIgnore]
    public string Pic
    {
        get { return pic; }
    }

    [JsonIgnore]
    public Sprite Avatar
    {
        get
        {
            if (avatar.IsNull())
            {
                GetAvatar();
            }

            return avatar;
        }
        private set
        {
            avatar = value;
            foreach (Image im in AvatarImages)
                im.sprite = value;
        }
    }

    [JsonIgnore]
    public Sprite Frame
    {
        get
        {
            if(frame.IsNull())
            {
                SetRandomFrame();
            }

            return frame;
        }
        private set
        {
            frame = value;

            foreach (Image im in FrameImages)
                im.sprite = value;
        }
    }

    [JsonIgnore]
    public int Level
    {
        get { return level; }
    }

    [JsonIgnore]
    public int Points
    {
        get { return points; }
    }
    #endregion
}