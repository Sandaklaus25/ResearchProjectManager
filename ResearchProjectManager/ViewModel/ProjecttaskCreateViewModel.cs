using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.ViewModels
{
    public class ProjectTaskCreateViewModel
    {
        [Required(ErrorMessage = "The project name is required.")]
        [StringLength(100, ErrorMessage = "The name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please provide a description.")]
        public string Description { get; set; }
    }
}