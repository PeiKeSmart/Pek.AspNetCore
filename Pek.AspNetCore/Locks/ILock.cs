﻿namespace Pek.Locks;

/// <summary>
/// 业务锁
/// </summary>
public interface ILock
{
    /// <summary>
    /// 锁定，成功锁定返回true，false代表之前已被锁定
    /// </summary>
    /// <param name="key">锁定标识</param>
    /// <param name="expiration">锁定时间间隔</param>
    /// <returns></returns>
    Boolean Lock(String key, Int32 expiration);

    /// <summary>
    /// 解除锁定
    /// </summary>
    void UnLock(Boolean autoUnLock);
}