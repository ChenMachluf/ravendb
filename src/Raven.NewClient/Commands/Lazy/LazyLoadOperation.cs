using System;
using System.Text;

using Raven.NewClient.Abstractions.Data;
using Raven.NewClient.Client.Commands;
using Raven.NewClient.Client.Data;
using Raven.NewClient.Client.Data.Queries;
using Raven.NewClient.Client.Document.Batches;
using Raven.NewClient.Client.Shard;
using Sparrow.Json;


namespace Raven.NewClient.Client.Commands.Lazy
{
    public class LazyLoadOperation<T> : ILazyOperation
    {
        private readonly LoadOperation loadOperation;
        private readonly string[] ids;
        private readonly string transformer;
        private readonly string[] includes;

        public LazyLoadOperation(
            LoadOperation loadOperation,
            string[] ids,
            string[] includes,
            string transformer = null)
        {
            this.loadOperation = loadOperation;
            this.ids = ids;
            this.includes = includes;
            this.transformer = transformer;
        }

        public LazyLoadOperation(
            LoadOperation loadOperation,
            string id)
        {
            this.loadOperation = loadOperation;
            ids = new[] { id };
        }

        public GetRequest CreateRequest()
        {
            var queryBuilder = new StringBuilder("?");
            includes.ApplyIfNotNull(include => queryBuilder.AppendFormat("&include={0}", include));
            ids.ApplyIfNotNull(id => queryBuilder.AppendFormat("&id={0}", Uri.EscapeDataString(id)));

            if (string.IsNullOrEmpty(transformer) == false)
                queryBuilder.AppendFormat("&transformer={0}", transformer);

            return new GetRequest
            {
                Url = "/docs",
                Query = queryBuilder.ToString()
            };
        }

        public object Result { get; set; }
        public QueryResult QueryResult { get; set; }
        public bool RequiresRetry { get; set; }

        public void HandleResponses(BlittableJsonReaderObject[] responses, ShardStrategy shardStrategy)
        {
            throw new NotImplementedException();
            /*var list = new List<LoadResult>(
                from response in responses
                let result = response.Result
                select new LoadResult
                {
                    Includes = result.Value<RavenJArray>("Includes").Cast<RavenJObject>().ToList(),
                    Results = result.Value<RavenJArray>("Results").Select(x => x as RavenJObject).ToList()
                });

            var capacity = list.Max(x => x.Results.Count);

            var finalResult = new LoadResult
            {
                Includes = new List<RavenJObject>(),
                Results = new List<RavenJObject>(Enumerable.Range(0, capacity).Select(x => (RavenJObject)null))
            };


            foreach (var multiLoadResult in list)
            {
                finalResult.Includes.AddRange(multiLoadResult.Includes);

                for (int i = 0; i < multiLoadResult.Results.Count; i++)
                {
                    if (finalResult.Results[i] == null)
                        finalResult.Results[i] = multiLoadResult.Results[i];
                }
            }
            RequiresRetry = loadOperation.SetResult(finalResult);
            if (RequiresRetry == false)
                Result = loadOperation.Complete<T>();
*/
        }

        public void HandleResponse(BlittableJsonReaderObject response)
        {

            object forceRetry;
            response.TryGetMember("ForceRetry", out forceRetry);

            if (( forceRetry!= null) && ((bool)forceRetry))
            {
                Result = null;
                RequiresRetry = true;
                return;
            }

            object result;
            response.TryGetMember("Result", out result);

            object include;
            object res;

            ((BlittableJsonReaderObject)result).TryGetMember("Results", out res);
            ((BlittableJsonReaderObject)result).TryGetMember("Includes", out include);

            var multiLoadResult = new GetDocumentResult()
            {
                Includes = (BlittableJsonReaderArray)include,
                Results = (BlittableJsonReaderArray)res
            };
            HandleResponse(multiLoadResult);
        }

        private void HandleResponse(GetDocumentResult loadResult)
        {
              loadOperation.SetResult(loadResult);
              if (RequiresRetry == false)
                  Result = loadOperation.GetDocuments<T>();
        }

        public IDisposable EnterContext()
        {
            return null;
            //throw new NotImplementedException();
            //return loadOperation.EnterLoadContext();
        }
    }
}