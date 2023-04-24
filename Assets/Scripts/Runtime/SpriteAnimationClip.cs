using UnityEngine;

[System.Serializable]
public class SpriteAnimationClip
{
    public string name;

    public Sprite[] sprites;

    public int fps = 24;

    public bool loop = true;

    public bool randomAtStart = false;
}