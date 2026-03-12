using System.ComponentModel.DataAnnotations;
namespace PlacementMentorshipPortal.Models;

public class YearRangeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        int year = (int)value;
        int currentYear = DateTime.Now.Year;
        int maxYear = currentYear + 1;

        if (year < 1948 || year > maxYear)
        {
            return new ValidationResult($"Year must be between 1948 and {maxYear}.");
        }

        return ValidationResult.Success;
    }
}