using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFormation : MonoBehaviour
{
    [SerializeField]
    GameObject m_Template;

    [SerializeField]
    int m_Width = 100;

    [SerializeField]
    int m_Height = 100;

    [SerializeField]
    float m_Space = 1f;

    void Start()
    {
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                GameObject gameObject = GameObject.Instantiate<GameObject>(m_Template);
                Vector3 pos = Vector3.zero;
                pos.x = pos.x + i * m_Space - (m_Space * m_Width * 0.5f);
                pos.z = pos.z + j * m_Space - (m_Space * m_Height * 0.5f);
                gameObject.transform.position = pos;
            }
        }
    }
}