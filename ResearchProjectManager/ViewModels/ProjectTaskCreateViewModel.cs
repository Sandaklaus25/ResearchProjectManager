using ResearchProjectManager.ViewModels.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ResearchProjectManager.ViewModels
{
    public class ProjectTaskCreateViewModel
    {
        public int CourseId { get; set; }
        public string ThemeColor { get; set; }

        [Required(ErrorMessage = "The project name is required.")]
        [StringLength(100, ErrorMessage = "The name cannot be longer than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a description.")]
        public string Description { get; set; } = string.Empty;
        public List<SubtaskInputDto> Subtasks { get; set; } = new();
    }

    
}