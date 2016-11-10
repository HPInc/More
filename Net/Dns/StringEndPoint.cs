// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Net;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
#else
using IPParser = System.Net.IPAddress;
#endif

namespace More
{
    /// <summary>
    /// A StringEndPoint is used when an endpoint starts out as a string, usually
    /// provided by the user or configuration at runtime.  This class will store the original
    /// string, and provides the ability to to parse/resolve the string as an ip address at any time
    /// and this structure will cache the resulting ip.
    ///
    /// If the ip is cached in this endpoint, then the library will exclusively use the ip for any
    /// network operations, otherwise, it will try to use the original raw string until it's necessary
    /// to parse or resolve the string as an ip.
    ///
    /// This structure is especially useful for proxies since some proxies require
    /// an ip address and some do not.  The proxy can then decide whether or not to resolve
    /// the ip or leave it as a raw string.
    /// 
    /// NOTE: since this is a struct with non-readonly members, alway use 'ref' when passing it to functions
    /// and never make it a readonly member of a struct/class.
    /// </summary>
    public struct StringEndPoint
    {
        public readonly String ipOrHost;
        public readonly UInt16 port;
        public readonly Boolean stringIsAnIP;
        public IPEndPoint ipEndPoint;

        public StringEndPoint(String ipOrHost, UInt16 port)
        {
            this.ipOrHost = ipOrHost;
            this.port = port;

            IPAddress ip;
            if (IPParser.TryParse(ipOrHost, out ip))
            {
                this.stringIsAnIP = true;
                this.ipEndPoint = new IPEndPoint(ip, port);
            }
            else
            {
                this.stringIsAnIP = false;
                this.ipEndPoint = null;
            }
        }
        public StringEndPoint(StringEndPoint other, UInt16 port)
        {
            this.ipOrHost = other.ipOrHost;
            this.port = port;
            this.stringIsAnIP = other.stringIsAnIP;
            if (other.ipEndPoint == null)
            {
                this.ipEndPoint = null;
            }
            else
            {
                this.ipEndPoint = new IPEndPoint(other.ipEndPoint.Address, port);
            }
        }

        /// <summary>
        /// Note: Since this is a struct and this function can modify the fields of the struct,
        /// make sure you are calling this on a "ref" version of the struct, and if it's a member
        /// of another object, make sure it IS NOT readonly.
        /// </summary>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        public void ForceIPResolution(PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            if (ipEndPoint == null)
            {
                ipEndPoint = new IPEndPoint(EndPoints.DnsResolve(ipOrHost, dnsPriorityQuery), port);
            }
        }
    }
}