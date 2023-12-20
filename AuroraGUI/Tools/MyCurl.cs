﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AuroraGUI.Tools
{
    class MyCurl
    {
        public class MWebClient : WebClient
        {
            public bool AllowAutoRedirect { get; set; } = true;
            public int TimeOut { get; set; } = 15000;

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                request.Timeout = TimeOut;
                if (request is HttpWebRequest webRequest)
                {
                    webRequest.AllowAutoRedirect = AllowAutoRedirect;
                    webRequest.KeepAlive = true;
                    webRequest.ProtocolVersion = new Version("1.1");
                }

                return request;
            }
        }

        public class MIpBkWebClient : WebClient
        {
            public bool AllowAutoRedirect { get; set; } = true;
            public int TimeOut { get; set; } = 15000;

            protected override WebRequest GetWebRequest(Uri address)
            {
                var ipAdd = IpTools.ResolveNameIpAddress(address.DnsSafeHost);
                var mAdd = new Uri(address.Scheme + Uri.SchemeDelimiter + ipAdd + address.AbsolutePath);
                var request = base.GetWebRequest(mAdd);
                request.Timeout = TimeOut;
                if (!(request is HttpWebRequest webRequest)) return request;
                webRequest.Host = address.DnsSafeHost;
                webRequest.AllowAutoRedirect = AllowAutoRedirect;
                webRequest.KeepAlive = true;
                webRequest.ProtocolVersion = new Version("1.1");
                return request;
            }
        }

        public class Http2Handler : WinHttpHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                System.Threading.CancellationToken cancellationToken)
            {
                request.Version = new Version("2.0");
                return base.SendAsync(request, cancellationToken);
            }
        }

        public static string GetStringByMWebClient(string url, bool proxyEnable = false, IWebProxy wProxy = null,
            bool allowRedirect = true)
        {
            MWebClient mWebClient = new MWebClient
            {
                Headers = {["User-Agent"] = "AuroraDNSC/0.1"},
                AllowAutoRedirect = allowRedirect,
                Proxy = proxyEnable ? wProxy : new WebProxy()
            };
            return mWebClient.DownloadString(url);
        }

        public static string GetStringByHttp2Client(string url, bool proxyEnable = false, IWebProxy wProxy = null,
            bool allowRedirect = true)
        {
            var mHttp2Handel = new Http2Handler
            {
                WindowsProxyUsePolicy =
                    !proxyEnable ? WindowsProxyUsePolicy.DoNotUseProxy : WindowsProxyUsePolicy.UseCustomProxy,
                AutomaticRedirection = allowRedirect,
                Proxy = !proxyEnable ? null : wProxy
            };
            HttpClient mHttpClient = new HttpClient(mHttp2Handel);
            mHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AuroraDNSC/0.1");
            return mHttpClient.GetStringAsync(url).Result;
        }

        public static byte[] GetDataByMWebClient(string url, bool proxyEnable = false, IWebProxy wProxy = null,
            bool allowRedirect = true, WebHeaderCollection headers = null)
        {
            if (headers == null)
            {
                headers = new WebHeaderCollection();
            }
            headers.Add(HttpRequestHeader.UserAgent, "AuroraDNSC/0.1");
            MWebClient mWebClient = new MWebClient
            {
                Headers = headers,
                AllowAutoRedirect = allowRedirect,
                Proxy = proxyEnable ? wProxy : new WebProxy()
            };
            return mWebClient.DownloadData(url);
        }

        public static byte[] GetDataByHttp2Client(string url, bool proxyEnable = false, IWebProxy wProxy = null,
            bool allowRedirect = true, WebHeaderCollection headers = null)
        {
            var mHttp2Handel = new Http2Handler
            {
                WindowsProxyUsePolicy =
                    !proxyEnable ? WindowsProxyUsePolicy.DoNotUseProxy : WindowsProxyUsePolicy.UseCustomProxy,
                AutomaticRedirection = allowRedirect,
                Proxy = !proxyEnable ? null : wProxy
            };
            HttpClient mHttpClient = new HttpClient(mHttp2Handel);
            mHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AuroraDNSC/0.1");
            if (headers != null)
            {
                foreach (string key in headers)
                {
                    mHttpClient.DefaultRequestHeaders.Add(key, headers[key]);
                }
            }
            return mHttpClient.GetByteArrayAsync(url).Result;
        }

        public static string GetString(string url, bool http2 = false, bool proxyEnable = false,
            IWebProxy wProxy = null, bool allowRedirect = true)
            => http2
                ? GetStringByHttp2Client(url, proxyEnable, wProxy, allowRedirect)
                : GetStringByMWebClient(url, proxyEnable, wProxy, allowRedirect);

        public static byte[] GetData(string url, bool http2 = false, bool proxyEnable = false,
            IWebProxy wProxy = null, bool allowRedirect = true, WebHeaderCollection headers = null)
            => http2
                ? GetDataByHttp2Client(url, proxyEnable, wProxy, allowRedirect, headers)
                : GetDataByMWebClient(url, proxyEnable, wProxy, allowRedirect, headers);
    }
}
