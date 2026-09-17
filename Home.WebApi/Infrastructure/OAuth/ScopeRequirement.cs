using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;

namespace Home.WebApi.Infrastructure.OAuth;

public class ScopeRequirement : IAuthorizationRequirement
{

    #region Constructors

    public ScopeRequirement(string scope)
        => this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));

    #endregion Constructors

    #region Properties

    public string Scope { get; }

    #endregion Properties

}

public class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{

    #region Fields

    private readonly IHttpContextAccessor m_HttpContextAccessor;

    #endregion Fields

    #region Constructors

    public ScopeHandler(IHttpContextAccessor httpContextAccessor)
        => this.m_HttpContextAccessor = httpContextAccessor;

    #endregion Constructors

    #region Methods

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        if (!context.User.Claims.Any())
            return Task.CompletedTask;

        var _ScopesClaim = context.User.FindFirst(FrameworkValues.IdentityClaimScopes);

        if (_ScopesClaim == null)
        {
            context.Fail(new(this, "Claim not found"));
            return Task.CompletedTask;
        }

        if (_ScopesClaim.Value.Split(",").Any(s => s.Equals(requirement.Scope)))
            context.Succeed(requirement);
        else
            context.Fail(new(this, "Scope not found"));

        return Task.CompletedTask;
    }

    #endregion Methods

}
