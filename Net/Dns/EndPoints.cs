// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
using UInt16Parser = System.MissingInCEUInt16Parser;
#else
using IPParser = System.Net.IPAddress;
using UInt16Parser = System.UInt16;
#endif

namespace More
{
    /// <summary>
    /// Functions used for DnsResolution and hostname to endpoint processing.
    /// </summary>
    public static class EndPoints
    {
        /// <summary>
        /// Resolves the DNS host name, and uses the given <paramref name="dnsPriorityQuery"/> to determine which address to use.
        /// </summary>
        /// <param name="host">host to resolve</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The resolved ip address and it's priority</returns>
        public static IPAddress DnsResolve(String host, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress[] addresses = hostEntry.AddressList;
            if (hostEntry == null || addresses == null || addresses.Length <= 0)
            {
                throw new DnsException("host");
            }
            var priorityValue = addresses.PrioritySelect(dnsPriorityQuery);
            if (priorityValue.value == null)
            {
                throw new NoSuitableAddressesException(host, addresses);
            }
            return priorityValue.value;
        }

        /// <summary>
        /// Resolves the DNS host name, and uses the given <paramref name="dnsPriorityQuery"/> to determine which address to use.
        /// Note: in order to distinguish between dns resolution errors and having no suitable addresses,
        /// it is recommended to not ignore any addresses in your priority query, but instead,
        /// give them low priority and check if the address family is suitable in the return value.
        /// </summary>
        /// <param name="host">host to resolve</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The resolved ip address with the highest priority</returns>
        public static PriorityValue<IPAddress> TryDnsResolve(String host, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress[] addresses = hostEntry.AddressList;
            if (hostEntry == null || addresses == null || addresses.Length <= 0)
            {
                return new PriorityValue<IPAddress>(Priority.Ignore, null);
            }
            return addresses.PrioritySelect(dnsPriorityQuery);
        }

        /// <summary>
        /// Removes and parses the ':port' prefix from the given string.
        /// </summary>
        /// <param name="ipOrHostAndPort">IP or Hostname with a ':port' postfix</param>
        /// <param name="port">The port number parsed from the host ':port' postfix.</param>
        /// <returns><paramref name="ipOrHostAndPort"/> with the ':port' postix removed.
        /// Also returns the ':port' postix in the form of a parsed port number.</returns>
        /// <exception cref="FormatException">If no ':port' postfix or if port is invalid.</exception>
        public static String SplitIPOrHostAndPort(String ipOrHostAndPort, out UInt16 port)
        {
            Int32 colonIndex = ipOrHostAndPort.IndexOf(':');
            if (colonIndex < 0)
            {
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostAndPort));
            }

            String portString = ipOrHostAndPort.Substring(colonIndex + 1);
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }
            return ipOrHostAndPort.Remove(colonIndex);
        }
        /// <summary>
        /// Removes and parses the optional ':port' prefix from the given string.
        /// </summary>
        /// <param name="ipOrHostAndPort">IP or Hostname with a ':port' postfix</param>
        /// <param name="port">The port number parsed from the host ':port' postfix.</param>
        /// <returns><paramref name="ipOrHostAndPort"/> with the ':port' postix removed.
        /// Also returns the ':port' postix in the form of a parsed port number.</returns>
        /// <exception cref="FormatException">If no ':port' postfix or if port is invalid.</exception>
        public static String SplitIPOrHostAndOptionalPort(String ipOrHostOptionalPort, ref UInt16 port)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex < 0)
            {
                return ipOrHostOptionalPort;
            }

            String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }
            return ipOrHostOptionalPort.Remove(colonIndex);
        }

        /// <summary>
        /// Tries to parse the string as an IPAddress.  If that fails, it will resolve
        /// it as a host name and select the best address using the given priority query.
        /// </summary>
        /// <param name="ipOrHost">An ip address or host name</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The parsed or resolved ip address.</returns>
        public static IPAddress ParseIPOrResolveHost(String ipOrHost, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPAddress ip;
            if (IPParser.TryParse(ipOrHost, out ip)) return ip;
            return DnsResolve(ipOrHost, dnsPriorityQuery);
        }

        /// <summary>
        /// It will check if <paramref name="ipOrHostOptionalPort"/>
        /// has a ':port' postfix.  If it does, it will parse that port number and use that instead of <paramref name="defaultPort"/>.
        /// Then tries to parse the string as an IPAddress.  If that fails, it will resolve
        /// it as a host name and select the best address using the given priority query.
        /// </summary>
        /// <param name="ipOrHostOptionalPort">IP or Hostname with an optional ':port' postfix</param>
        /// <param name="defaultPort">The default port to use if there is no ':port' postfix</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns></returns>
        public static IPEndPoint ParseIPOrResolveHostWithOptionalPort(String ipOrHostOptionalPort, UInt16 defaultPort, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                // NOTE: It would be nice to parse this without creating another string
                String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out defaultPort))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
                ipOrHostOptionalPort = ipOrHostOptionalPort.Remove(colonIndex);
            }

            return new IPEndPoint(ParseIPOrResolveHost(ipOrHostOptionalPort, dnsPriorityQuery), defaultPort);
        }
    }
    /// <summary>
    /// Thrown when DNS fails.
    /// </summary>
    public class DnsException : Exception
    {
        public readonly String host;
        public DnsException(String host)
            : base(String.Format("DNS found no addresses for '{0}'", host))
        {
            this.host = host;
        }
    }
    /// <summary>
    /// Thrown when DNS for a hostname succeeded, but none of the addresses were suitable.
    /// </summary>
    public class NoSuitableAddressesException : Exception
    {
        public static String GetAddressFamilyStrings(IEnumerable<IPAddress> addresses)
        {
            if (addresses == null)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            HashSet<AddressFamily> found = new HashSet<AddressFamily>();
            foreach(var address in addresses)
            {
                if (!found.Contains(address.AddressFamily))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(address.AddressFamily);
                    found.Add(address.AddressFamily);
                }
            }
            return builder.ToString();
        }

        public readonly String host;
        public readonly IPAddress[] unsuitableAddresses;
        public NoSuitableAddressesException(String host, IPAddress[] unsuitableAddresses)
            : base(String.Format("DNS for '{0}' succeeded, but no suitable address were found in the set ({1})", host, GetAddressFamilyStrings(unsuitableAddresses)))
        {
            this.host = host;
            this.unsuitableAddresses = unsuitableAddresses;
        }
    }
}
