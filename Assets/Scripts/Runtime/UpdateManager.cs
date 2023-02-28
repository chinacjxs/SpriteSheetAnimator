using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdateReceiver
{
    bool isActiveAndEnabled { get; }

    bool isDestroyed { get; }

    void OnUpdateReceive(string param);
}

public abstract class MonoUpdateReceiver : MonoBehaviour, IUpdateReceiver
{
    public abstract string Name { get; }

    Dictionary<string, int> valuePairs = new Dictionary<string, int>();
    
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
    
    public void SetUpdateState(bool state, string param)
    {
        int identity = 0;
        if (!valuePairs.TryGetValue(param, out identity))
            valuePairs.Add(param, identity);

        if(state && identity == 0)
            valuePairs[param] = UpdateManager.Instance.GetManager(Name).DoUpdate(this, param);
        if (!state && identity != 0)
        {
            UpdateManager.Instance.GetManager(Name).RemoveUpdate(identity, param);
            valuePairs[param] = 0;
        }
    }

    public bool GetUpdateState(string param)
    {
        if (valuePairs.TryGetValue(param, out int identity))
            return identity != 0;
        return false;
    }
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

    float threshold = 0.003f;

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

    public int DoUpdate(IUpdateReceiver receiver ,string param)
    {
        if (receiver == null)
            return 0;

        LinkedList<IUpdateReceiver> list = null;
        if (param == kStrUpdate)
            list = m_Update;
        else if (param == kStrLateUpdate)
            list = m_LateUpdate;
        else if (param == kStrFixedUpdate)
            list = m_FixedUpdate;

        m_ReceiverPairs.Add(++identity, list.AddLast(receiver));
        return identity;
    }

    public void RemoveUpdate(int id,string param)
    {
        if (m_ReceiverPairs.TryGetValue(id,out LinkedListNode<IUpdateReceiver> receiver))
        {
            m_ReceiverPairs.Remove(id);

            if (param == kStrUpdate)
            {
                if (receiver == m_UpdateFirst)
                    m_UpdateFirst = receiver.Next;
                m_Update.Remove(receiver);
            }
            else if(param == kStrLateUpdate)
            {
                if (receiver == m_LateUpdateFirst)
                    m_LateUpdateFirst = receiver.Next;
                m_LateUpdate.Remove(receiver);
            }
            else if(param == kStrFixedUpdate)
            {
                if (receiver == m_FixedUpdateFirst)
                    m_FixedUpdateFirst = receiver.Next;
                m_FixedUpdate.Remove(receiver);
            }
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

    void CallUpdateMethod(LinkedList<IUpdateReceiver> list,ref LinkedListNode<IUpdateReceiver> first,string param)
    {
        LinkedListNode<IUpdateReceiver> last = list.Last;
        LinkedListNode<IUpdateReceiver> next = null;

        lastUpdateTime = Time.realtimeSinceStartup;
        first = (first == null || first == last) ? list.First : first;

        while (first != last)
        {
            var obj = first.Value;
            next = first.Next;

            if (obj == null || obj.isDestroyed)
                list.Remove(first);
            else
            {
                if (obj.isActiveAndEnabled)
                {
                    try
                    {
                        obj.OnUpdateReceive(param);
                    }
                    catch (Exception)
                    {
                        list.Remove(first);
                    }
                }
            }

            first = next;

            if (Busy)
                break;
        }
    }
}