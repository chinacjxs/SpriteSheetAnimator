using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdateReceiver
{
    bool isActiveAndEnabled { get; }

    bool isDestroyed { get; }

    void OnUpdateReceive(string str);
}

public abstract class MonoUpdateReceiver : MonoBehaviour, IUpdateReceiver
{
    public abstract string Name { get; }

    int m_UpdateIdentity = 0;

    int m_LateUpdateIdentity = 0;

    int m_FixedUpdateIdentity = 0;

    public bool UpdateState
    {
        get
        {
            return m_UpdateIdentity != 0;
        }
        set
        {
            if(value && m_UpdateIdentity == 0)
                m_UpdateIdentity = UpdateManager.Instance.GetManager(Name).DoUpdate(this);

            if(!value && m_UpdateIdentity != 0)
            {
                UpdateManager.Instance.GetManager(Name).RemoveUpdate(m_UpdateIdentity);
                m_UpdateIdentity = 0;
            }
        }
    }

    public bool LateUpdateState
    {
        get
        {
            return m_LateUpdateIdentity != 0;
        }
        set
        {
            if (value && m_LateUpdateIdentity == 0)
                m_LateUpdateIdentity = UpdateManager.Instance.GetManager(Name).DoLateUpdate(this);

            if (!value && m_LateUpdateIdentity != 0)
            {
                UpdateManager.Instance.GetManager(Name).RemoveLateUpdate(m_LateUpdateIdentity);
                m_LateUpdateIdentity = 0;
            }
        }
    }

    public bool FixedUpdateState
    {
        get
        {
            return m_FixedUpdateIdentity != 0;
        }
        set
        {
            if (value && m_FixedUpdateIdentity == 0)
                m_FixedUpdateIdentity = UpdateManager.Instance.GetManager(Name).DoFixedUpdate(this);

            if (!value && m_FixedUpdateIdentity != 0)
            {
                UpdateManager.Instance.GetManager(Name).RemoveFixedUpdate(m_FixedUpdateIdentity);
                m_FixedUpdateIdentity = 0;
            }
        }
    }

    bool m_isDestroyed = false;

    bool IUpdateReceiver.isActiveAndEnabled => isActiveAndEnabled;

    bool IUpdateReceiver.isDestroyed => m_isDestroyed;

    void IUpdateReceiver.OnUpdateReceive(string str)
    {
        if (str == UpdateManager.kStrUpdate)
            OnUpdate();
        else if (str == UpdateManager.kStrLateUpdate)
            OnLateUpdate();
        else if (str == UpdateManager.kStrFixedUpdate)
            OnFixedUpdate();
    }

    protected virtual void OnDestroy()
    {
        m_isDestroyed = true;
    }

    protected virtual void OnUpdate() { }

    protected virtual void OnLateUpdate() { }

    protected virtual void OnFixedUpdate() { }
}

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

    LinkedList<IUpdateReceiver> m_Update = new LinkedList<IUpdateReceiver>();

    LinkedList<IUpdateReceiver> m_LateUpdate = new LinkedList<IUpdateReceiver>();

    LinkedList<IUpdateReceiver> m_FixedUpdate = new LinkedList<IUpdateReceiver>();

    Dictionary<int, LinkedListNode<IUpdateReceiver>> m_ReceiverPairs = new Dictionary<int, LinkedListNode<IUpdateReceiver>>();

    int identity = 0;

    float lastUpdateTime = 0f;

    LinkedListNode<IUpdateReceiver> m_UpdateFirst;

    LinkedListNode<IUpdateReceiver> m_LateUpdateFirst;

    LinkedListNode<IUpdateReceiver> m_FixedUpdateFirst;

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

    public int DoUpdate(IUpdateReceiver receiver)
    {
        if (receiver == null)
            return 0;
        m_ReceiverPairs.Add(++identity,m_Update.AddLast(receiver));
        return identity;
    }

    public int DoLateUpdate(IUpdateReceiver receiver)
    {
        if (receiver == null)
            return 0;
        m_ReceiverPairs.Add(++identity, m_LateUpdate.AddLast(receiver));
        return identity;
    }

    public int DoFixedUpdate(IUpdateReceiver receiver)
    {
        if (receiver == null)
            return 0;
        m_ReceiverPairs.Add(++identity, m_FixedUpdate.AddLast(receiver));
        return identity;
    }

    public void RemoveUpdate(int id)
    {
        if (m_ReceiverPairs.TryGetValue(id,out LinkedListNode<IUpdateReceiver> receiver))
        {
            if (receiver == m_UpdateFirst)
                m_UpdateFirst = receiver.Next;
            m_Update.Remove(receiver);
        }
    }

    public void RemoveLateUpdate(int id)
    {
        if (m_ReceiverPairs.TryGetValue(id, out LinkedListNode<IUpdateReceiver> receiver))
        {
            if (receiver == m_LateUpdateFirst)
                m_LateUpdateFirst = receiver.Next;
            m_LateUpdate.Remove(receiver);
        }
    }

    public void RemoveFixedUpdate(int id)
    {
        if (m_ReceiverPairs.TryGetValue(id, out LinkedListNode<IUpdateReceiver> receiver))
        {
            if (receiver == m_FixedUpdateFirst)
                m_FixedUpdateFirst = receiver.Next;
            m_FixedUpdate.Remove(receiver);
        }
    }

    public bool Busy => Time.realtimeSinceStartup - lastUpdateTime >= 0.003f;

    void Awake()
    {
        if (__instance == null)
        {
            __instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Update()
    {
        CallUpdateMethod(m_Update,ref m_UpdateFirst,kStrUpdate);
    }

    void LateUpdate()
    {
        CallUpdateMethod(m_LateUpdate,ref m_LateUpdateFirst,kStrLateUpdate);
    }

    void FixedUpdate()
    {
        CallUpdateMethod(m_FixedUpdate,ref m_FixedUpdateFirst,kStrFixedUpdate);
    }

    void CallUpdateMethod(LinkedList<IUpdateReceiver> list,ref LinkedListNode<IUpdateReceiver> pFirst,string str)
    {
        LinkedListNode<IUpdateReceiver> pLast = list.Last;
        LinkedListNode<IUpdateReceiver> pNext = null;

        lastUpdateTime = Time.realtimeSinceStartup;
        pFirst = (pFirst == null || pFirst == pLast) ? list.First : pFirst;

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
                        obj.OnUpdateReceive(str);
                    }
                    catch (Exception)
                    {
                        list.Remove(pFirst);
                    }
                }
            }

            pFirst = pNext;

            if (Busy)
                break;
        }
    }
}