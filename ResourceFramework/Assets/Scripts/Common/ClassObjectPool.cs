using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T:class,new()
{
    protected Stack<T> pool = new Stack<T>();
    protected int maxCount = 0; //最大对象个数，<=0表示不限个数
    protected int noRecyleCount = 0;

    public ClassObjectPool(int maxCount){
        this.maxCount=maxCount;
        for(int i=0;i<maxCount;i++){
            pool.Push(new T());
        }
    }

    public T Spawn(bool createIfPoolEmpty)
    {
        T t = null;
        if (pool.Count > 0)
        {
            t = pool.Pop();
        }

        if (t == null && createIfPoolEmpty)
        {
           t = new T();
        }

        if (t != null)
        {
            noRecyleCount++;
        }

        return t;
    }

    public bool Recyle(T t)
    {
        if (t == null)
        {
            return false;
        }

        noRecyleCount--;

        if (maxCount > 0 && pool.Count >= maxCount)
        {
            t = null;
            return false;
        }
        else
        {
            pool.Push(t);
            return true;
        }
    }
}
