﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFinal.Owin
{
    public struct OwinKeys
    {
        public const string RequestHeaders = "owin.RequestHeaders";
        public const string RequestMethod = "owin.RequestMethod";
        public const string RequestPath = "owin.RequestPath";
        public const string RequestPathBase = "owin.RequestPathBase";
        public const string RequestProtocol = "owin.RequestProtocol";
        public const string RequestQueryString = "owin.RequestQueryString";
        public const string RequestScheme = "owin.RequestScheme";
        public const string RequestBody = "owin.RequestBody";
        public const string ResponseHeaders = "owin.ResponseHeaders";
        public const string ResponseStatusCode = "owin.ResponseStatusCode";
        public const string ResponseBody = "owin.ResponseBody";
        public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";
        public const string ResponseProtocol = "owin.ResponseProtocol";
        public const string CallCancelled = "owin.CallCancelled";
        public const string Version = "owin.Version";
        public const string RemoteIpAddress = "server.RemoteIpAddress";
        public const string RemotePort = "server.RemotePort";
        public const string LocalIpAddress="server.LocalIpAddress";
        public const string LocalPort = "server.LocalPort";
        public const string IsLocal = "server.IsLocal";
    }
}
