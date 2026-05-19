using AutoMapper;
using CleanArchitecture.Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Home.WebApi.Controllers;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Octet, "application/json;charset=UTF-8", "multipart/form-data", "application/x-www-form-urlencoded;charset=UTF-8")]
public class BaseController : ControllerBase
{

    #region Fields

    private IMapper m_Mapper;
    private Pipeline m_Pipeline;

    #endregion Fields

    #region Properties

    public IMapper Mapper => this.m_Mapper ??= this.HttpContext.RequestServices.GetService<IMapper>();
    public Pipeline Pipeline => this.m_Pipeline ??= this.HttpContext.RequestServices.GetRequiredService<Pipeline>();
    public ServiceFactory ServiceFactory => this.HttpContext.RequestServices.GetRequiredService<ServiceFactory>();

    #endregion Properties

}
