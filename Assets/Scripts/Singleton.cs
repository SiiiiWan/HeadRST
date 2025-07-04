using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    protected static bool isDestoyable = false;

    private static bool m_applicationIsQuitting = false;

    public static T GetInstance()
    {
        if (m_applicationIsQuitting) return null;

        if (instance == null)
        {
            instance = FindObjectOfType<T>();
            if (instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                instance = obj.AddComponent<T>();
            }
        }
        return instance;
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (!isDestoyable) DontDestroyOnLoad(gameObject);
        }
        else if (instance != this as T) Destroy(gameObject);
        else if (!isDestoyable) DontDestroyOnLoad(gameObject);

    }

    private void OnApplicationQuit()

    {
        m_applicationIsQuitting = true;
    }

}