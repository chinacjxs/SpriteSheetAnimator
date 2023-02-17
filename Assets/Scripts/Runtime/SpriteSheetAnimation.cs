﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class SpriteAnimationClip
{
    public string name;

    public Sprite[] sprites;

    public int fps = 24;

    public bool loop = true;
}

[CreateAssetMenu()]
public class SpriteSheetAnimation : ScriptableObject,ISerializationCallbackReceiver
{
    [SerializeField]
    List<SpriteAnimationClip> m_Clips = new List<SpriteAnimationClip>();

    Dictionary<string, SpriteAnimationClip> pairs = new Dictionary<string, SpriteAnimationClip>();

    public void SetAllSprites(UnityEngine.Object[] sprites,string pattern)
    {
        Dictionary<string, List<Sprite>> dict1 = new Dictionary<string, List<Sprite>>();
        Dictionary<string, List<int>> dict2 = new Dictionary<string, List<int>>();

        for (int i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i] as Sprite;
            if (sprite == null)
                continue;
            var match = Regex.Match(sprite.name, pattern);
            if (match.Success)
            {
                string name;
                int index;
                try
                {
                    name = match.Groups["name"].Value;
                    index = int.Parse(match.Groups["index"].Value);
                }
                catch (Exception e)
                {
                    throw new Exception(sprite.name,e);
                }
                
                List<Sprite> list1;
                if (!dict1.TryGetValue(name, out list1))
                {
                    list1 = new List<Sprite>();
                    dict1.Add(name, list1);
                }

                List<int> list2;
                if (!dict2.TryGetValue(name, out list2))
                {
                    list2 = new List<int>();
                    dict2.Add(name, list2);
                }

                list1.Add(sprite);
                list2.Add(index);

                if (list2.Count >= 2)
                {
                    var v1 = list1[list1.Count - 1];
                    var v2 = list2[list2.Count - 1];
                    var j = list2.Count - 2;

                    while (j >= 0)
                    {
                        if (v2 >= list2[j])
                            break;

                        list2[j + 1] = list2[j];
                        list1[j + 1] = list1[j];
                        j--;
                    }

                    list1[j + 1] = v1;
                    list2[j + 1] = v2;
                }
            }
        }

        m_Clips.Clear();
        foreach (var item in dict1)
        {
            SpriteAnimationClip clip = new SpriteAnimationClip();
            clip.name = item.Key;
            clip.sprites = item.Value.ToArray();
            m_Clips.Add(clip);
        }
    }
    
    public SpriteAnimationClip GetSpriteAnimationClip(string name)
    {
        if (pairs.TryGetValue(name, out SpriteAnimationClip clip))
            return clip;
        return null;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {

    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        pairs.Clear();

        for (int i = 0; i < m_Clips.Count; i++)
            pairs.Add(m_Clips[i].name, m_Clips[i]);
    }
}