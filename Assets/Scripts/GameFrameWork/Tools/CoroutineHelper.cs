using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools
{
    //协程协助者
    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        public Coroutine StartCoroutineWrapper(IEnumerator enumerator)
        {
            return StartCoroutine(enumerator);
        }

        public void StopCoroutineWrapper(IEnumerator enumerator)
        {
            StopCoroutine(enumerator);
        }
    }
}

