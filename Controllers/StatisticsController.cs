using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController :ControllerBase
    {
        private readonly StatisticsService _statisticsService;

        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }
        // ===========================================================
        // =                    DOANH THU                            =
        // ===========================================================
        [HttpGet("revenue/date")]
        public async Task<IActionResult> GetRevenueByDate([FromQuery] DateTime date)
        {
            var result = await _statisticsService.GetRevenueByDateAsync(date);
            return Ok(result);
        }
        [HttpGet("revenue/date-range")]
        public async Task<IActionResult> GetRevenueByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _statisticsService.GetRevenueByDateRangeAsync(startDate, endDate);
            return Ok(result);
        }
        [HttpGet("revenue/month")]
        public async Task<IActionResult> GetRevenueByMonth([FromQuery] DateTime month)
        {
            var result = await _statisticsService.GetRevenueByMonthAsync(month);
            return Ok(result);
        }
        [HttpGet("revenue/month-range")]
        public async Task<IActionResult> GetRevenueByMonthRange([FromQuery] DateTime startMonth, [FromQuery] DateTime endMonth)
        {
            var result = await _statisticsService.GetRevenueByMonthRangeAsync(startMonth, endMonth);
            return Ok(result);
        }
        // ===========================================================
        // =                  TỶ LỆ LẤP ĐẦY                         =
        // ===========================================================
        [HttpGet("occupancy/date")]
        public async Task<IActionResult> GetOccupancyByDate(
            [FromQuery] DateTime date)
        {
            var result = await _statisticsService.GetOccupancyByDateAsync(date);
            return Ok(result);
        }
        [HttpGet("occupancy/date-range")]
        public async Task<IActionResult> GetOccupancyByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _statisticsService.GetOccupancyByDateRangeAsync(startDate, endDate);
            return Ok(result);
        }
        [HttpGet("occupancy/month")]
        public async Task<IActionResult> GetOccupancyByMonth([FromQuery] DateTime month)
        {
            var result = await _statisticsService.GetOccupancyByMonthAsync(month);
            return Ok(result);
        }
        [HttpGet("occupancy/month-range")]
        public async Task<IActionResult> GetOccupancyByMonthRange([FromQuery] DateTime startMonth, [FromQuery] DateTime endMonth)
        {
            var result = await _statisticsService.GetOccupancyByMonthRangeAsync(startMonth, endMonth);
            return Ok(result);
        }
    }
}
