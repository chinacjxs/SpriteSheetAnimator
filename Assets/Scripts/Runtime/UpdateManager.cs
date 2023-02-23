using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    static UpdateManager __instance;

    public const string kStrUpdate = "Update";
    public const string kStrLateUpdate = "LateUpdate";
    public const string kStrFixedUpdate = "FixedUpdate";

    public static UpdateManager Instance
    {
        get
        {
            if (__instance == null)
                __instance = CreateInstance(typeof(UpdateManager).Name, null);
            return __instance;
        }
    }

    static UpdateManager CreateInstance(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        UpdateManager launcher = obj.AddComponent<UpdateManager>();
        return launcher;
    }

    static Dictionary<string, UpdateManager> m_Managers = new Dictionary<string, UpdateManager>();

    LinkedList<IUpdatable> m_Update = new LinkedList<IUpdatable>();

    LinkedList<IUpdatable> m_LateUpdate = new LinkedList<IUpdatable>();

    LinkedList<IUpdatable> m_FixedUpdate = new LinkedList<IUpdatable>();

    public UpdateManager GetManager(string name)
    {
        UpdateManager launcher = null;
        if (!m_Managers.TryGetValue(name, out launcher))
        {
            launcher = CreateInstance(name, __instance.transform);
            m_Managers.Add(name, launcher);
        }
        return launcher;
    }

    public void DoUpdate(IUpdatable updatable)
    {
        m_Update.AddLast(updatable);
    }

    public void DoLateUpdate(IUpdatable updatable)
    {
        m_LateUpdate.AddLast(updatable);
    }

    public void DoFixedUpdate(IUpdatable updatable)
    {
        m_FixedUpdate.AddLast(updatable);
    }

    void Awake()
    {
        if (__instance == null)
        {
            __instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Update() { CallUpdateMethod(m_Update,kStrUpdate); }

    void LateUpdate() { CallUpdateMethod(m_LateUpdate,kStrLateUpdate); }

    void FixedUpdate() { CallUpdateMethod(m_FixedUpdate,kStrFixedUpdate); }

    void CallUpdateMethod(LinkedList<IUpdatable> list,string type)
    {
        LinkedListNode<IUpdatable> pFirst = list.First;
        LinkedListNode<IUpdatable> pLast = list.Last;
        LinkedListNode<IUpdatable> pNext = null;

        while (pFirst != pLast)
        {
            var obj = pFirst.Value;
            pNext = pFirst.Next;

            if (obj == null || obj.isDestroyed)
                list.Remove(pFirst);
            else
            {
                if (obj.isActiveAndEnabled)
                {
                    try
                    {
                        obj.OnUpdate(type);
                    }
                    catch (Exception)
                    {
                        list.Remove(pFirst);
                    }
                }
            }           
            pFirst = pNext;
        }
    }
}