using HotelManagement.Data;
using HotelManagement.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Enums;

namespace HotelManagement.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;
        public DashboardService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<DashboardResponseDto> GetOverviewAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var startOfToday = now.Date;
            var endOfToday = startOfToday.AddDays(1);
            // ROOM
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active);
            var availableRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active
                                                                   && r.Status == RoomEnums.Available);
            var occupiedRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active
                                                                  && r.Status == RoomEnums.Occupied);
            var maintenanceRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active
                                                                     && r.Status == RoomEnums.Maintenance);
            // BOOKING
            var pendingBookings = await _context.Bookings.CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                                       && b.Status == BookingEnums.Pending);
            var confirmedBookings = await _context.Bookings.CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                                    && b.Status == BookingEnums.Confirmed);
            var checkedInBookings = await _context.Bookings.CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                                    && b.Status == BookingEnums.CheckedIn);
            var completedBookings = await _context.Bookings.CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                                    && b.Status == BookingEnums.Completed);
            var cancelledBookings = await _context.Bookings.CountAsync(b => b.Status == BookingEnums.Cancelled);
            // CUSTOMER
            var totalCustomers = await _context.Customers.CountAsync(c => c.User != null 
                                                                  && c.User.IsDeleted == ActiveEnums.Active);
            var newCustomersThisMonth = await _context.Customers
                                                      .CountAsync(c => c.CreateAt >= startOfMonth 
                                                               && c.CreateAt < endOfMonth 
                                                               && c.User != null 
                                                               && c.User.IsDeleted == ActiveEnums.Active);
            // CHECK-IN / CHECK-OUT
            var todayCheckIns = await _context.Bookings
                                              .CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                            && b.ExpectedCheckIn >= startOfToday
                                                            && b.ExpectedCheckIn < endOfToday
                                                            && b.Status != BookingEnums.Cancelled
                                                            && b.Status != BookingEnums.Rejected);
            var todayCheckOuts = await _context.Bookings
                                               .CountAsync(b => b.IsDeleted == ActiveEnums.Active
                                                             && b.ExpectedCheckOut >= startOfToday
                                                             && b.ExpectedCheckOut < endOfToday
                                                             && b.Status != BookingEnums.Cancelled
                                                             && b.Status != BookingEnums.Rejected);
            // SERVICE
            var totalServices = await _context.Services.CountAsync(s => s.IsDeleted == ActiveEnums.Active);
            var serviceUsagesThisMonth = await _context.BookingServices
                                                       .CountAsync(bs => bs.CreateAt >= startOfMonth 
                                                                      && bs.CreateAt < endOfMonth);
            // PROMOTION
            // Khuyến mãi đang được áp dụng
            var activePromotions = await _context.Promotions
                                                 .CountAsync(p => p.IsDeleted == ActiveEnums.Active
                                                               && p.StartDate <= now
                                                               && p.EndDate >= now);
            // Khuyến mãi sắp hết hạn
            var expiringPromotions = await _context.Promotions
                                                   .CountAsync(p => p.IsDeleted == ActiveEnums.Active
                                                                 && p.EndDate >= now
                                                                 && p.EndDate <= now.AddDays(7));
            // Khuyến mãi đã hết hạn
            var expiredPromotions = await _context.Promotions
                                                  .CountAsync(p => p.IsDeleted == ActiveEnums.Active
                                                                && p.EndDate < now);
            // RETURN
            return new DashboardResponseDto
            {
                // Room
                TotalRooms = totalRooms,
                AvailableRooms = availableRooms,
                OccupiedRooms = occupiedRooms,
                MaintenanceRooms = maintenanceRooms,
                // Booking
                PendingBookings = pendingBookings,
                ConfirmedBookings = confirmedBookings,
                CheckedInBookings = checkedInBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                // Customer
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,
                // Check in / check out
                TodayCheckIns = todayCheckIns,
                TodayCheckOuts = todayCheckOuts,
                // Service
                TotalServices = totalServices,
                ServiceUsagesThisMonth = serviceUsagesThisMonth,
                // Promotion
                ActivePromotions = activePromotions,
                ExpiringPromotions = expiringPromotions,
                ExpiredPromotions = expiredPromotions
            };
        }
    }
}
