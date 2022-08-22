using Azure;
using Azure.Data.Tables;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot
{
    public class DummyBotConversationCache : IBotConversationCache
    {
        public Task AddConversationReferenceToCache(Activity activity) => Task.CompletedTask;

        public Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, string serviceUrl, string channelId) => Task.CompletedTask;

        public bool ContainsUserId(string aadId) => false;

        public CachedUserAndConversationData GetCachedUser(string aadObjectId) => null;

        public List<CachedUserAndConversationData> GetCachedUsers() => new List<CachedUserAndConversationData>();

        public Task RemoveFromCache(string aadObjectId) => Task.CompletedTask;
    }
    public class AzureTableBotConversationCache : IBotConversationCache
    {
        #region Privates & Constructors

        const string TABLE_NAME = "ConversationCache";
        private ConcurrentDictionary<string, CachedUserAndConversationData> _userIdConversationCache = new();
        private Config _config;
        private TableClient _tableClient;

        public AzureTableBotConversationCache(Config config)
        {
            _config = config;
            this._tableClient = new TableClient(
                config.Storage,
                TABLE_NAME);

            // Dev only: make sure the Azure Storage emulator is running or this will fail
            _tableClient.CreateIfNotExists();

            var queryResultsFilter = _tableClient.Query<CachedUserAndConversationData>(filter: $"PartitionKey eq '{CachedUserAndConversationData.PartitionKeyVal}'");
            foreach (var qEntity in queryResultsFilter)
            {
                _userIdConversationCache.AddOrUpdate(qEntity.RowKey, qEntity, (key, newValue) => qEntity);
                Console.WriteLine($"{qEntity.RowKey}: {qEntity}");
            }

        }
        #endregion

        public async Task RemoveFromCache(string aadObjectId)
        {
            CachedUserAndConversationData u = null;
            if (_userIdConversationCache.TryGetValue(aadObjectId, out u))
            {
                _userIdConversationCache.TryRemove(aadObjectId, out u);
            }

            await _tableClient.DeleteEntityAsync(CachedUserAndConversationData.PartitionKeyVal, aadObjectId);
        }


        /// <summary>
        /// App installed for user & now we have a conversation reference to cache for future chat threads.
        /// </summary>
        public async Task AddConversationReferenceToCache(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            await AddOrUpdateUserAndConversationId(conversationReference, activity.ServiceUrl, activity.ChannelId);
        }

        public async Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, string serviceUrl, string channelId)
        {
            CachedUserAndConversationData u = null;
            var userId = conversationReference.User.AadObjectId ?? conversationReference.User.Id;
            if (!_userIdConversationCache.TryGetValue(userId, out u))
            {

                // Have not got in memory cache
                Response<CachedUserAndConversationData> entityResponse = null;
                try
                {
                    entityResponse = await _tableClient.GetEntityAsync<CachedUserAndConversationData>(CachedUserAndConversationData.PartitionKeyVal, conversationReference.User.Id);
                }
                catch (RequestFailedException ex)
                {
                    if (ex.ErrorCode == "ResourceNotFound")
                    {
                        // No worries
                    }
                    else
                    {
                        throw;
                    }
                }

                if (entityResponse == null)
                {
                    // Not in storage account either. Add there
                    u = new CachedUserAndConversationData()
                    {
                        RowKey = userId,
                        ServiceUrl = serviceUrl,
                        ChannelId = channelId
                    };
                    u.ConversationId = conversationReference.Conversation.Id;
                    _tableClient.AddEntity(u);
                }
                else
                {
                    u = entityResponse.Value;
                }
            }

            // Update memory cache
            _userIdConversationCache.AddOrUpdate(userId, u, (key, newValue) => u);
        }


        public List<CachedUserAndConversationData> GetCachedUsers()
        {
            return _userIdConversationCache.Values.ToList();
        }

        public CachedUserAndConversationData GetCachedUser(string aadObjectId)
        {
            return _userIdConversationCache.Values.Where(u => u.RowKey == aadObjectId).SingleOrDefault();
        }

        public bool ContainsUserId(string aadId)
        {
            return _userIdConversationCache.ContainsKey(aadId);
        }
    }


    public interface IBotConversationCache
    {

        public Task RemoveFromCache(string aadObjectId);


        /// <summary>
        /// App installed for user & now we have a conversation reference to cache for future chat threads.
        /// </summary>
        public Task AddConversationReferenceToCache(Activity activity);

        public Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, string serviceUrl, string channelId);


        public List<CachedUserAndConversationData> GetCachedUsers();

        public CachedUserAndConversationData GetCachedUser(string aadObjectId);
        public bool ContainsUserId(string aadId);
    }

    /// <summary>
    /// Table storage or memory cache for user
    /// </summary>
    public class CachedUserAndConversationData : ITableEntity
    {
        public static string PartitionKeyVal => "Users";
        public string PartitionKey { get => PartitionKeyVal; set { return; } }

        /// <summary>
        /// Azure AD ID
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        public string ConversationId { get; set; }

        public string ChannelId { get; set; }
    }
}
