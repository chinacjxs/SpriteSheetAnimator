using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusader : MonoBehaviour
{
    [SerializeField]
    SpriteSheetAnimator animator;

    [SerializeField]
    float m_Horizontal;
    [SerializeField]
    float m_Vertical;

    [SerializeField]
    float m_Angle;

    [SerializeField]
    string m_CurrentActionName = "idle";

    [SerializeField]
    string m_CurrentAnimationName;

    static List<Action> m_Actions = new List<Action>();
    static Coroutine m_Coroutine = null;

    void Start()
    {
        m_Actions.Add(InputLogic);
        if (m_Coroutine == null)
            m_Coroutine = StartCoroutine(InputProcess());
        
    }

    IEnumerator InputProcess()
    {
        while (true)
        {
            yield return null;
            foreach (var item in m_Actions)
                item();
        }
    }

    void InputLogic()
    {
        m_Horizontal = Input.GetAxis("Horizontal");
        m_Vertical = Input.GetAxis("Vertical");

        m_Angle = Vector2.SignedAngle(Vector2.down, new Vector2(m_Horizontal, m_Vertical));
        m_Angle = m_Angle > 0 ? m_Angle : 360f + m_Angle;

        if (Mathf.Approximately(m_Horizontal, 0) && Mathf.Approximately(m_Vertical, 0))
            return;

        string animationName = string.Format("{0}_{1}", m_CurrentActionName, GetDirection(m_Angle));
        if (m_CurrentAnimationName != animationName)
        {
            m_CurrentAnimationName = animationName;
            animator.Play(m_CurrentAnimationName);
        }
    }

    int GetDirection(float angle)
    {
        var dir = 0;
        if (angle >= 0f && angle < 45f)
            dir = 0;
        else if (angle >= 45f && angle < 90f)
            dir = 1;
        else if (angle >= 90f && angle < 135f)
            dir = 2;
        else if (angle >= 135f && angle < 180f)
            dir = 3;
        else if (angle >= 180f && angle < 225f)
            dir = 4;
        else if (angle >= 225f && angle < 270f)
            dir = 5;
        else if (angle >= 270f && angle < 315f)
            dir = 6;
        else if (angle >= 315f && angle < 360f)
            dir = 7;

        return dir;
    }
}
