﻿using System;
using System.Net.Http;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Http;
using Raven.Client.Json.Converters;
using Sparrow.Json;

namespace Raven.Client.Server.Operations
{
    public class DisableResourceToggleOperation : IServerOperation<DisableResourceToggleResult>
    {
        private readonly string _databaseName;
        private readonly bool _ifDisableRequest;

        public DisableResourceToggleOperation(string databaseName, bool ifDisableRequest)
        {
            if(databaseName == null)
                throw new ArgumentNullException(nameof(databaseName));

            _databaseName = databaseName;
            _ifDisableRequest = ifDisableRequest;
        }

        public RavenCommand<DisableResourceToggleResult> GetCommand(DocumentConventions conventions,
            JsonOperationContext context)
        {
            return new DisableResourceToggleCommand(_databaseName, _ifDisableRequest);
        }

        public class DisableResourceToggleCommand : RavenCommand<DisableResourceToggleResult>
        {

            private readonly string _databaseName;
            private readonly bool _ifDisableRequest;

            public DisableResourceToggleCommand(string databaseName, bool ifDisableRequest)
            {
                _databaseName = databaseName;
                _ifDisableRequest = ifDisableRequest;

                ResponseType = RavenCommandResponseType.Array;
            }

            public override HttpRequestMessage CreateRequest(ServerNode node, out string url)
            {
                var toggle = _ifDisableRequest ? "disable" : "enable";
                url = $"{node.Url}/admin/databases/{toggle}?name={_databaseName}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                };

                return request;
            }
        
            public override void SetResponse(BlittableJsonReaderObject response, bool fromCache)
            {
                if (response == null)
                    ThrowInvalidResponse();
               
            }

            public override void SetResponse(BlittableJsonReaderArray response, bool fromCache)
            {
                if (response == null)
                    ThrowInvalidResponse();

                var resultObject = response[0] as BlittableJsonReaderObject;
                Result = JsonDeserializationClient.DisableResoureceToggleResult(resultObject);

            }

            public override bool IsReadRequest => false;
        }
    }
}