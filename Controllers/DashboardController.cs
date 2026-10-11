using HotelManagement.DTOs.Dashboard;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;
        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet]
        public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
        {
            var result = await _dashboardService.GetOverviewAsync();
            return Ok(result);
        }
    }
}
