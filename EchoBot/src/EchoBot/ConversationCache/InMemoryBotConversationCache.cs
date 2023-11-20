using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot.ConversationCache;


public class InMemoryBotConversationCache : IBotConversationCache
{
    private Dictionary<string, CachedUserAndConversationData> _userIdConversationCache = new();

    public Task AddConversationReferenceToCache(Activity activity)
    {
        var conversationReference = activity.GetConversationReference();
        AddOrUpdateUserAndConversationId(conversationReference, activity.ServiceUrl, activity.ChannelId);
        return Task.CompletedTask;
    }

    public Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, string serviceUrl, string channelId)
    {
        var userId = conversationReference.User.AadObjectId ?? conversationReference.User.Id;
        var u = new CachedUserAndConversationData()
        {
            RowKey = userId,
            ServiceUrl = serviceUrl,
            ChannelId = channelId,
            ConversationId = conversationReference.Conversation.Id
        };

        if (_userIdConversationCache.ContainsKey(userId))
        {
            _userIdConversationCache[userId] = u;
        }
        else
        {
            _userIdConversationCache.Add(userId, u);
        }
        return Task.CompletedTask;
    }

    public bool ContainsUserId(string aadId)
    {
        return _userIdConversationCache.ContainsKey(aadId);
    }

    public CachedUserAndConversationData? GetCachedUser(string aadObjectId)
    {
        return _userIdConversationCache.Values.Where(u => u.RowKey == aadObjectId).SingleOrDefault();
    }

    public List<CachedUserAndConversationData> GetCachedUsers()
    {
        return _userIdConversationCache.Values.ToList();
    }

    public Task RemoveFromCache(string aadObjectId)
    {
        CachedUserAndConversationData? u = null;
        if (_userIdConversationCache.TryGetValue(aadObjectId, out u))
        {
            _userIdConversationCache.Remove(aadObjectId);
        }
        return Task.CompletedTask;
    }
}