using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusader : MonoBehaviour
{
    [SerializeField]
    SpriteSheetAnimator animator;

    [SerializeField]
    string m_DefaultAnimationName;

    void Start()
    {
        animator.Play(m_DefaultAnimationName);
    }
}
