using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLauncher : MonoBehaviour
{
    static UpdateLauncher __instance;

    public static UpdateLauncher Instance
    {
        get
        {
            if (__instance == null)
                __instance = CreateInstance(typeof(UpdateLauncher).Name, null);
            return __instance;
        }
    }

    static UpdateLauncher CreateInstance(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        UpdateLauncher launcher = obj.AddComponent<UpdateLauncher>();
        return launcher;
    }

    static Dictionary<string, UpdateLauncher> m_Launchers = new Dictionary<string, UpdateLauncher>();

    class BehaviourCallback
    {
        MonoBehaviour m_MonoBehaviour;

        Action m_Callback;

        UpdateLauncher m_Launcher;

        public BehaviourCallback(UpdateLauncher launcher,MonoBehaviour monoBehaviour, Action callback)
        {
            m_MonoBehaviour = monoBehaviour;
            m_Callback = callback;
            m_Launcher = launcher;
        }

        public bool IsSleeping()
        {
            if (m_MonoBehaviour.isActiveAndEnabled)
                return false;
            return true;
        }

        public bool IsValid()
        {
            if (m_MonoBehaviour == null)
                return false;
            return true;
        }

        public void Invoke()
        {
            m_Callback();
        }
    }

    LinkedList<BehaviourCallback> m_Update = new LinkedList<BehaviourCallback>();

    LinkedList<BehaviourCallback> m_LateUpdate = new LinkedList<BehaviourCallback>();

    LinkedList<BehaviourCallback> m_FixedUpdate = new LinkedList<BehaviourCallback>();

    public UpdateLauncher GetLauncher(string name)
    {
        UpdateLauncher launcher = null;
        if (!m_Launchers.TryGetValue(name, out launcher))
        {
            launcher = CreateInstance(name, __instance.transform);
            m_Launchers.Add(name, launcher);
        }
        return launcher;
    }

    public void DoUpdate(MonoBehaviour behaviour, Action callback)
    {
        m_Update.AddLast(new BehaviourCallback(this,behaviour, callback));
    }

    public void DoLateUpdate(MonoBehaviour behaviour, Action callback)
    {
        m_LateUpdate.AddLast(new BehaviourCallback(this,behaviour, callback));
    }

    public void DoFixedUpdate(MonoBehaviour behaviour, Action callback)
    {
        m_FixedUpdate.AddLast(new BehaviourCallback(this,behaviour, callback));
    }

    void Awake()
    {
        if (__instance == null)
        {
            __instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Update() { CallUpdateMethod(m_Update); }

    void LateUpdate() { CallUpdateMethod(m_LateUpdate); }

    void FixedUpdate() { CallUpdateMethod(m_FixedUpdate); }

    void CallUpdateMethod(LinkedList<BehaviourCallback> list)
    {
        LinkedListNode<BehaviourCallback> pFirst = list.First;
        LinkedListNode<BehaviourCallback> pLast = list.Last;
        LinkedListNode<BehaviourCallback> pNext = null;

        while (pFirst != pLast)
        {
            pNext = pFirst.Next;
            if (pFirst.Value.IsValid())
            {
                if (!pFirst.Value.IsSleeping())
                {
                    try
                    {
                        pFirst.Value.Invoke();
                    }
                    catch (Exception)
                    {
                        list.Remove(pFirst);
                    }
                }
            }
            else
                list.Remove(pFirst);
            pFirst = pNext;
        }
    }
}