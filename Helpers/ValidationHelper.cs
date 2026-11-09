using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.Helpers;

public static class ValidationHelper
{
    public static bool TryValidate<T>(T model,out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
       var validationContext= new ValidationContext(model!);

       return Validator.TryValidateObject(model!,validationContext,validationResults,true);
    }
}