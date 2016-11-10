// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.Sockets;

namespace More
{
    /// <summary>
    /// A set of standard priority functions for evaluating the priority to select the given ip address.
    /// These functions can be used whenever a PriorityQuery&lt;IPAddres&gt; is expected.
    /// </summary>
    public static class DnsPriority
    {
        public static Priority IPv4ThenIPv6(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: return Priority.Highest;
                case AddressFamily.InterNetworkV6: return Priority.SecondHighest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv4Only(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv6ThenIPv4(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: return Priority.SecondHighest;
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv6Only(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv4ThenIPv6ThenOther(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: return Priority.Highest;
                case AddressFamily.InterNetworkV6: return Priority.SecondHighest;
                default: return Priority.ThirdHighest;
            }
        }
        public static Priority IPv6ThenIPv4ThenOther(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork: return Priority.SecondHighest;
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return Priority.ThirdHighest;
            }
        }
    }
}