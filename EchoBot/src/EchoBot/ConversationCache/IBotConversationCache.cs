using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot.ConversationCache;


public interface IBotConversationCache
{

    public Task RemoveFromCache(string aadObjectId);


    /// <summary>
    /// App installed for user & now we have a conversation reference to cache for future chat threads.
    /// </summary>
    public Task AddConversationReferenceToCache(Activity activity);

    public Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, string serviceUrl, string channelId);


    public List<CachedUserAndConversationData> GetCachedUsers();

    public CachedUserAndConversationData? GetCachedUser(string aadObjectId);
    public bool ContainsUserId(string aadId);
}
