﻿using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Sparrow.Json;

namespace Raven.Client.Documents.Commands
{
    public abstract class RavenCommand<TResult>
    {
        public readonly string ServerUrl;
        public readonly string Database;

        public CancellationToken CancellationToken = CancellationToken.None;

        protected RavenCommand(string serverUrl, string database)
        {
            ServerUrl = serverUrl;
            Database = database;
        }

        public JsonOperationContext Context;

        public TResult Result;

        public abstract HttpRequestMessage CreateRequest();
        public abstract void SetResponse(BlittableJsonReaderObject response);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value);
        }

        public static void EnsureIsNotNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{name} cannot be null or empty", name);
        }
    }
}