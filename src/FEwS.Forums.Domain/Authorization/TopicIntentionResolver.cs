using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization.AccessManagement;
using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.Authorization;

internal class TopicIntentionResolver :
    IIntentionResolver<TopicIntention>,
    IIntentionResolver<TopicIntention, Topic>
{
    public bool IsAllowed(IIdentity subject, TopicIntention intention)
    {
        return intention switch
        {
            TopicIntention.Create => subject.IsAuthenticated(),
            TopicIntention.CreateComment => throw new NotImplementedException(),
            _ => false
        };
    }

    public bool IsAllowed(IIdentity subject, TopicIntention intention, Topic target)
    {
        return intention switch
        {
            TopicIntention.CreateComment => subject.IsAuthenticated(),
            TopicIntention.Create => throw new NotImplementedException(),
            _ => false
        };
    }
}