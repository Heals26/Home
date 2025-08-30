namespace Home.Application.Services.Validation;

public class HomeInputPortValidationFailure(List<HomeInputPortValidationFailure.ValidationError> errors)
{

    #region Properties

    public List<ValidationError> Errors { get; } = errors;

    #endregion Properties

    #region Methods

    public Dictionary<string, string[]>? GetValidationErrors()
        => this.Errors
            ?.GroupBy(e => e.PropertyName)
            ?.ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Error).ToArray()
            );

    #endregion Methods

    #region Nested Classes

    public class ValidationError(string propertyName, string error)
    {

        #region Properties

        public string Error { get; } = error;
        public string PropertyName { get; } = propertyName;

        #endregion Properties

    }

    #endregion Nested Classes

}