using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Values;

namespace Home.WebApi.Infrastructure.AutoMapper.Resolvers;

/// <summary>
/// Seconds remaining on an access token. This lives in a resolver rather than inline in the
/// profile because AutoMapper constructs a Profile without DI, so a profile cannot reach the
/// clock — a resolver is resolved from the container at map time and can.
/// </summary>
/// <typeparam name="TDestination">The grant response being built.</typeparam>
public class TokenExpiresInResolver<TDestination>(TimeProvider timeProvider)
    : IValueResolver<UserAuthentication, TDestination, long>
{

    #region Methods

    public long Resolve(UserAuthentication source, TDestination destination, long destMember, ResolutionContext context)
        => (long)(source.DateSetUTC.Add(FrameworkValues.AccessTokenLifetime)
            - timeProvider.GetUtcNow().UtcDateTime).TotalSeconds;

    #endregion Methods

}
