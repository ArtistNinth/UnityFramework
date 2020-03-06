using System;
using System.Collections.Generic;

public class ObjectManager : Singleton<ObjectManager>
{
    protected Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();

    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class,new()
    {
        Type currentType = typeof(T);
        ClassObjectPool<T> pool;
        if (classPoolDic.ContainsKey(currentType))
        {
            pool = classPoolDic[currentType] as ClassObjectPool<T>;
        }
        else
        {
            pool = new ClassObjectPool<T>(maxCount);
            classPoolDic.Add(currentType, pool);
        }
        return pool;
    }
}