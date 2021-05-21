using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace OnlineBankingLibrary.Services
{
    internal static class ProxyManager<IType>
    {
        internal static ConcurrentDictionary<string, ChannelFactory<IType>> proxies = new ConcurrentDictionary<string, ChannelFactory<IType>>();

        internal static IType GetProxy(string key, Binding binding, EndpointAddress endpoint)
        {
            return proxies.GetOrAdd(key, m => new ChannelFactory<IType>(binding, endpoint)).CreateChannel();
        }

        internal static bool RemoveProxy(string key)
        {
            ChannelFactory<IType> proxy;
            return proxies.TryRemove(key, out proxy);
        }
    }
}
