using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour,IUpdatable
{
    [SerializeField]
    SpriteRenderer m_SpriteRenderer;

    [SerializeField]
    SpriteSheetAnimation m_SpriteAnimation;

    [SerializeField]
    bool m_IgnoreTimeScale = false;

    [SerializeField]
    bool m_StartAtRandomFrame = false;

    [SerializeField]
    float m_Speed = 1.0f;

    SpriteAnimationClip m_SpriteAnimationClip;

    float m_Accumulation = 0;

    int m_SequenceNumber = 0;

    bool m_IsPlaying = false;

    const string kSpriteSheetAnimator = "SpriteSheetAnimator";

    bool m_IsDestroyed = false;

    void Start()
    {
        UpdateManager.Instance.GetManager(kSpriteSheetAnimator).DoUpdate(this);
    }

    //void Update()
    //{
    //    UpdateAnimation();
    //}

    public void Play(string name)
    {
        var animationClip = m_SpriteAnimation.GetSpriteAnimationClip(name);
        if (animationClip != null)
        {
            m_IsPlaying = true;
            m_SpriteAnimationClip  = animationClip;

            m_SequenceNumber = m_StartAtRandomFrame ? UnityEngine.Random.Range(0,animationClip.sprites.Length - 1) : 0;
            Sample(m_SequenceNumber);
        }
    }

    void Sample(int index)
    {
        if(m_SpriteAnimationClip != null)
            m_SpriteRenderer.sprite = m_SpriteAnimationClip.sprites[index];
    }

    void UpdateAnimation()
    {
        if(m_IsPlaying)
        {
            float interval = 1f / m_SpriteAnimationClip.fps;
            float deltaTime = m_IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            m_Accumulation = m_Accumulation + deltaTime * m_Speed;
            while (m_Accumulation >= interval)
            {
                m_Accumulation = m_Accumulation - interval;

                var index = m_SequenceNumber + 1;
                if (index >= m_SpriteAnimationClip.sprites.Length)
                {
                    if (!m_SpriteAnimationClip.loop)
                    {
                        m_IsPlaying = false;
                        return;
                    }
                    index = 0;
                }
                m_SequenceNumber = index;
                Sample(m_SequenceNumber);
            }
        } 
    }

    void OnDestroy()
    {
        m_IsDestroyed = true;
    }

    void IUpdatable.OnUpdate(string type)
    {
        UpdateAnimation();
    }

    bool IUpdatable.isActiveAndEnabled => isActiveAndEnabled;

    bool IUpdatable.isDestroyed => m_IsDestroyed;
}