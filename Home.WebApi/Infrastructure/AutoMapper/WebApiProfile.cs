using AutoMapper;
using Home.Application.UseCases.ApiAuditing;
using Home.Domain.Entities;

namespace Home.WebApi.Infrastructure.AutoMapper;

public class WebApiProfile : Profile
{

    #region Constructors

    public WebApiProfile()
        => this.CreateMap<CreateApiAuditEntry, ApiAuditEntry>()
            .ConvertUsing((dto, entity) => new ApiAuditEntry
            {
                UserID = dto.AuthenticationData.UserID,
                ClientApplicationID = dto.AuthenticationData.ClientApplicationID,

                ActionName = dto.ActionData.ActionName,
                CreatedResourceID = dto.ActionData.CreatedResourceID,
                Details = dto.ActionData.Details?.Substring(0, dto.ActionData.Details.Length > 4000 ? 4000 : dto.ActionData.Details.Length),

                RemoteIPAddress = dto.RequestData.RemoteIPAddress,
                RequestBody = dto.RequestData.RequestBody,
                RequestReceivedOnUTC = dto.RequestData.RequestReceivedOnUTC,
                RequestUri = dto.RequestData.RequestUri,
                UserAgent = dto.RequestData.UserAgent,
                Version = dto.RequestData.Version,

                HttpResponseStatusCode = dto.ResponseData.HttpResponseStatusCode,
                ResponseSentOnUTC = dto.ResponseData.ResponseSentOnUTC,
            });

    #endregion Constructors

}
