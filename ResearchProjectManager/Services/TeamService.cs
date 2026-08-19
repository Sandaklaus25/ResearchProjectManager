using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class TeamService
    {
        private readonly ApplicationDbContext _context;

        public TeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Team> CreateTeamAsync(Team team )
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _context.Teams
                            .Include(t => t.UserTeams)
                                .ThenInclude(ut => ut.User) 
                            .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                            .Include(t => t.UserTeams)
                                .ThenInclude(ut => ut.User) 
                            .ToListAsync();
        }

        public async Task AddUserToTeamAsync(int userId, int teamId)
        {
            var user = await _context.Users.FindAsync(userId);
            var team = await _context.Teams.FindAsync(teamId);

            if (user == null || team == null) return;

            var userTeam = new UserTeam
            {
                UserId = userId,
                TeamId = teamId
            };
            _context.UserTeams.Add(userTeam);
            await _context.SaveChangesAsync();
        }
    }
}
