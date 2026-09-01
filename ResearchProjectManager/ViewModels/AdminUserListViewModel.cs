using ResearchProjectManager.ViewModels.Dto;

namespace ResearchProjectManager.ViewModels
{
    public class AdminUserListViewModel
    {
        public List<AdminPanelCreateUserDto> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
    }
}
