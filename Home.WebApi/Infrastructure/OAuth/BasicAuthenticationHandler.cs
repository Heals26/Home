using Home.Application.Services.Persistence;
using Microsoft.AspNetCore.Authentication;

namespace Home.WebApi.Infrastructure.OAuth;

public class BasicAuthenticationHandler(IPersistenceContext persistenceContext) : AuthenticationHandler<AuthenticationSchemeOptions>
{


}
