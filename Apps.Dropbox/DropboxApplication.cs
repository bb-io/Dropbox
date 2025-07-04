﻿using Apps.Dropbox.Auth.OAuth2;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Dropbox
{
    public class DropboxApplication : BaseInvocable, IApplication, ICategoryProvider
    {
        private readonly Dictionary<Type, object> _typesInstances;

        public IEnumerable<ApplicationCategory> Categories
        {
            get => [ApplicationCategory.FileManagementAndStorage];
            set { }
        }
        
        public DropboxApplication(InvocationContext invocationContext) : base(invocationContext)
        {
            _typesInstances = CreateTypesInstances();
        }

        public T GetInstance<T>()
        {
            if (!_typesInstances.TryGetValue(typeof(T), out var value))
            {
                throw new InvalidOperationException($"Instance of type '{typeof(T)}' not found");
            }
            return (T)value;
        }
    
        private Dictionary<Type, object> CreateTypesInstances()
        {
            return new Dictionary<Type, object>
            {
                { typeof(IOAuth2AuthorizeService), new OAuth2AuthorizeService(InvocationContext) },
                { typeof(IOAuth2TokenService), new OAuth2TokenService(InvocationContext) }
            };
        }
    }
}
