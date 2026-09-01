using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchProjectManager.Services;

[Authorize(Roles = "Instructor,TA")]
public class TeamController : Controller
{
    private readonly TeamService _teamService;

    public TeamController(TeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeamSafe(int teamId)
    {
        var result = await _teamService.DeleteTeamSafeAsync(teamId);
        if (!result.Success)
        {
            return Json(new { success = false, message = result.Message });
        }

        return Json(new { success = true });
    }
}