using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization.AccessManagement;

namespace FEwS.Forums.Domain.Authorization;

internal class AccountIntentionResolver : IIntentionResolver<AccountIntention>
{
    public bool IsAllowed(IIdentity subject, AccountIntention intention) => intention switch
    {
        AccountIntention.SignOut => subject.IsAuthenticated(),
        _ => false,
    };
}