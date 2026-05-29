namespace Home.WebApi.UseCases.ActivityContents.GetActivityContents;

public record GetActivityContentsApiResponse(List<GetActivityContentDto> Contents);

public record GetActivityContentDto(
    long ActivityContentID,
    string Content,
    int Sequence);
