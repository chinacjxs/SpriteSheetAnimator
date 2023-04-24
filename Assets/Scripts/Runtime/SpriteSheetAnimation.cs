using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[CreateAssetMenu()]
public class SpriteSheetAnimation : ScriptableObject,ISerializationCallbackReceiver
{
    [SerializeField]
    List<SpriteAnimationClip> m_Clips = new List<SpriteAnimationClip>();

    Dictionary<string, SpriteAnimationClip> m_Pairs = new Dictionary<string, SpriteAnimationClip>();

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

        foreach (var item in dict1)
        {
            SpriteAnimationClip clip;
            if(!m_Pairs.TryGetValue(item.Key,out clip))
            {
                clip = new SpriteAnimationClip() { name = item.Key };
                m_Pairs.Add(item.Key, clip);
            }
            clip.sprites = item.Value.ToArray();
        }

        List<string> list = new List<string>();
        foreach (var item in m_Pairs)
        {
            if (!dict1.ContainsKey(item.Key))
                list.Add(item.Key);
        }

        foreach (var item in list)
            m_Pairs.Remove(item);
    }
    
    public SpriteAnimationClip GetSpriteAnimationClip(string name)
    {
        if (m_Pairs.TryGetValue(name, out SpriteAnimationClip clip))
            return clip;
        return null;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        m_Pairs.Clear();
        for (int i = 0; i < m_Clips.Count; i++)
            m_Pairs.Add(m_Clips[i].name, m_Clips[i]);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        m_Clips.Clear();
        foreach (var item in m_Pairs)
            m_Clips.Add(item.Value);
    }
}
