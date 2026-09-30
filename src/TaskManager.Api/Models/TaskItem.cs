using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Models;

public class TaskItem : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Название")]
    [Required(ErrorMessage = "Укажите название задачи")]
    [StringLength(200, ErrorMessage = "Название должно содержать не больше 200 символов")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    [StringLength(2000, ErrorMessage = "Описание не должно быть длиннее 2000 символов")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Срок")]
    public DateTime DueDate { get; set; }

    [Display(Name = "Выполнена")]
    public bool IsDone { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate == default || DueDate.Year < 2000 || DueDate.Year > 2100)
        {
            yield return new ValidationResult(
                "Укажите срок выполнения между 2000 и 2100 годом",
                new[] { nameof(DueDate) });
        }
    }
}
