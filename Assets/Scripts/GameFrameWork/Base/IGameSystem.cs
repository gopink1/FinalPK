using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGameSystem
{
    protected GameBase m_GameBase = null;
    public IGameSystem(GameBase gameBase)
    {
        m_GameBase = gameBase;
    }

    public abstract void Init();
    public abstract void Release();
    public abstract void Update();
}
