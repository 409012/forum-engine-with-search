using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization.AccessManagement;

namespace FEwS.Forums.Domain.Authorization;

internal class ForumIntentionResolver : IIntentionResolver<ForumIntention>
{
    public bool IsAllowed(IIdentity subject, ForumIntention intention)
    {
        return intention switch
        {
            ForumIntention.Create => subject.IsAuthenticated(),
            _ => false
        };
    }
}