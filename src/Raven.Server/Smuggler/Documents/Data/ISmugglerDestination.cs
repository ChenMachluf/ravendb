﻿using System;
using Raven.NewClient.Abstractions.Indexing;
using Raven.NewClient.Data.Indexes;
using Raven.NewClient.Client.Indexing;
using Raven.NewClient.Client.Smuggler;
using Raven.Server.Documents;
using Raven.Server.Documents.Indexes;
using Sparrow.Json;

namespace Raven.Server.Smuggler.Documents.Data
{
    public interface ISmugglerDestination
    {
        IDisposable Initialize(DatabaseSmugglerOptions options, SmugglerResult result, long buildVersion);
        IDocumentActions Documents();
        IDocumentActions RevisionDocuments();
        IIndexActions Indexes();
        ITransformerActions Transformers();
        IIdentityActions Identities();
    }

    public interface IDocumentActions : INewDocumentActions, IDisposable
    {
        void WriteDocument(Document document);
    }

    public interface INewDocumentActions
    {
        JsonOperationContext GetContextForNewDocument();
    }

    public interface IIndexActions : IDisposable
    {
        void WriteIndex(IndexDefinitionBase indexDefinition, IndexType indexType);
        void WriteIndex(IndexDefinition indexDefinition);
    }

    public interface ITransformerActions : IDisposable
    {
        void WriteTransformer(TransformerDefinition transformerDefinition);
    }

    public interface IIdentityActions : IDisposable
    {
        void WriteIdentity(string key, long value);
    }
}