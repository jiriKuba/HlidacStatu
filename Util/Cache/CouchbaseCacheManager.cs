﻿using System;
using System.Collections.Generic;
using System.Linq;
using Devmasters.Cache.V20;

namespace HlidacStatu.Util.Cache
{

    public class CouchbaseCacheManager<T, Key>
        : Manager<T,Key, CouchbaseCache<T>>
        where T : class
    {
    
        public CouchbaseCacheManager(string keyPrefix, System.Func<Key, T> func, TimeSpan expiration)
            : base(keyPrefix, func,expiration)
        {
        }
        protected override CouchbaseCache<T> getTCacheInstance(Key key, TimeSpan expiration, Func<Key, T> contentFunc)
        {

            return new CouchbaseCache<T>(expiration, this.keyPrefix + key.ToString(), (o) => contentFunc.Invoke(key));
        }


        public static CouchbaseCacheManager<T, Key> GetSafeInstance(string instanceName, System.Func<Key, T> func, TimeSpan expiration)
        {
            lock (instancesLock)
            {
                string instanceFullName = instanceName + "|" + typeof(T).ToString() + "|" + typeof(Key).ToString();

                if (!instances.ContainsKey(instanceFullName))
                {
                    instances[instanceFullName] = new CouchbaseCacheManager<T, Key>(instanceName, func, expiration);
                }
                return (CouchbaseCacheManager<T, Key>)instances[instanceFullName];
            }
        }



    }
}
