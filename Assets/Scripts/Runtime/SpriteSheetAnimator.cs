using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer m_SpriteRenderer;

    [SerializeField]
    SpriteSheetAnimation m_SpriteAnimation;

    [SerializeField]
    bool m_IgnoreTimeScale = false;


    SpriteAnimationClip m_SpriteAnimationClip;

    float m_AccumulationTime = 0;

    int m_Index = 0;


    void Update()
    {
        UpdateAnimation();
    }

    public void Play(string name)
    {
        var animationClip = m_SpriteAnimation.GetSpriteAnimationClip(name);
        if (animationClip == null)
            return;

        m_Index = animationClip.randomAtStart ? UnityEngine.Random.Range(0, animationClip.sprites.Length - 1) : 0;
        m_SpriteAnimationClip = animationClip;
        m_AccumulationTime = 0f;

        Sample(m_Index);
    }

    public void Stop()
    {
        m_SpriteAnimationClip = null;
    }

    void Sample(int index)
    {
        if (m_SpriteAnimationClip == null)
            return;

        if (index < 0 || index >= m_SpriteAnimationClip.sprites.Length)
            return;

        m_SpriteRenderer.sprite = m_SpriteAnimationClip.sprites[index];
    }

    void UpdateAnimation()
    {
        if (m_SpriteAnimationClip == null)
            return;

        float interval = 1f / m_SpriteAnimationClip.fps;
        float deltaTime = m_IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        m_AccumulationTime = m_AccumulationTime + deltaTime;

        while (m_AccumulationTime >= interval)
        {
            m_AccumulationTime = m_AccumulationTime - interval;
            m_Index = m_Index + 1;

            if (m_Index >= m_SpriteAnimationClip.sprites.Length)
            {
                if (m_SpriteAnimationClip.loop)
                    m_Index = 0;
                else
                {
                    Stop();
                    return;
                }
            }
            Sample(m_Index);
        }
    }
}