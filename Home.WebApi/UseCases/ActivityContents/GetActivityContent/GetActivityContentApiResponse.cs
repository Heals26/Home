namespace Home.WebApi.UseCases.ActivityContents.GetActivityContent;

public record GetActivityContentApiResponse(
    long ActivityContentID,
    string Content,
    int Sequence);
