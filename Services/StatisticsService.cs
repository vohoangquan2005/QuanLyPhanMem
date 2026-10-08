using HotelManagement.Data;
using HotelManagement.DTOs.Statistics;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class StatisticsService
    {
        private readonly AppDbContext _context;
        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }
        // ===========================================================
        // =                     DOANH THU                          =
        // ===========================================================

        // Thống kê doanh thu của một ngày
        public async Task<RevenueDailyStatisticsDto> GetRevenueByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            var payments = await _context.Payments
                                         .Where(p => p.Status == PaymentEnums.Success
                                                  && p.PaymentDate >= startDate
                                                  && p.PaymentDate < endDate)
                                         .ToListAsync();
            return new RevenueDailyStatisticsDto
            {
                Date = startDate,
                Revenue = payments.Sum(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount),
                PaymentCount = payments.Count
            };
        }
        // Thống kê doanh thu theo từng ngày trong khoảng thời gian
        public async Task<List<RevenueDailyStatisticsDto>> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;
            if (startDate > endDate)
                throw new BadRequestException("Ngày bắt đầu không được lớn hơn ngày kết thúc!");
            var endExclusive = endDate.AddDays(1);
            var payments = await _context.Payments
                                         .Where(p => p.Status == PaymentEnums.Success
                                                  && p.PaymentDate >= startDate 
                                                  && p.PaymentDate < endExclusive)
                                         .ToListAsync();
            var result = new List<RevenueDailyStatisticsDto>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var nextDate = date.AddDays(1);
                var dailyPayments = payments.Where(p => p.PaymentDate >= date 
                                                && p.PaymentDate < nextDate)
                                            .ToList();
                result.Add(new RevenueDailyStatisticsDto
                {
                    Date = date,
                    Revenue = dailyPayments.Sum(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount),
                    PaymentCount = dailyPayments.Count
                });
            }
            return result;
        }
        // Thống kê doanh thu của một tháng
        public async Task<RevenueMonthlyStatisticsDto> GetRevenueByMonthAsync(DateTime month)
        {
            var startDate = new DateTime( month.Year,month.Month,1);
            var endDate = startDate.AddMonths(1);
            var payments = await _context.Payments
                                         .Where(p => p.Status == PaymentEnums.Success
                                                  && p.PaymentDate >= startDate
                                                  && p.PaymentDate < endDate)
                                         .ToListAsync();
            return new RevenueMonthlyStatisticsDto
            {
                Month = startDate,
                Revenue = payments.Sum(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount),
                PaymentCount = payments.Count
            };
        }
        // Thống kê doanh thu theo từng tháng trong khoảng thời gian
        public async Task<List<RevenueMonthlyStatisticsDto>> GetRevenueByMonthRangeAsync(DateTime startMonth, DateTime endMonth)
        {
            var currentMonth = new DateTime(startMonth.Year, startMonth.Month, 1);
            var lastMonth = new DateTime(endMonth.Year, endMonth.Month, 1);
            if (currentMonth > lastMonth)
                throw new BadRequestException("Tháng bắt đầu không được lớn hơn tháng kết thúc!");
            var payments = await _context.Payments
                                         .Where(p => p.Status == PaymentEnums.Success
                                                  && p.PaymentDate >= currentMonth 
                                                  && p.PaymentDate < lastMonth.AddMonths(1))
                                         .ToListAsync();
            var result = new List<RevenueMonthlyStatisticsDto>();
            while (currentMonth <= lastMonth)
            {
                var nextMonth = currentMonth.AddMonths(1);
                var monthlyPayments = payments.Where(p => p.PaymentDate >= currentMonth 
                                                  && p.PaymentDate < nextMonth)
                                              .ToList();
                result.Add(new RevenueMonthlyStatisticsDto
                {
                    Month = currentMonth,
                    Revenue = monthlyPayments.Sum(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount),
                    PaymentCount = monthlyPayments.Count
                });
                currentMonth = nextMonth;
            }
            return result;
        }
        // ===========================================================
        // =                  TỶ LỆ LẤP ĐẦY                         =
        // ===========================================================

        // Tỷ lệ lấp đầy của một ngày
        public async Task<OccupancyDailyStatisticsDto> GetOccupancyByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            // Tổng số phòng đang hoạt động
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active);
            // Các BookingDetail có thời gian lưu trú giao với ngày cần thống kê
            var occupiedRooms = await _context.BookingDetails
                                             .Where(bd => bd.Booking!.IsDeleted == ActiveEnums.Active 
                                                 && bd.Booking.Status != BookingEnums.Cancelled 
                                                 && bd.Booking.Status != BookingEnums.Rejected
                                                 && bd.Booking.ExpectedCheckIn < endDate 
                                                 && bd.Booking.ExpectedCheckOut > startDate
                                            )
                                            .Select(bd => bd.RoomId)
                                            .Distinct()
                                            .CountAsync();
            decimal occupancyRate = 0;
            if (totalRooms > 0)
                occupancyRate = (decimal)occupiedRooms / totalRooms * 100;
            return new OccupancyDailyStatisticsDto
            {
                Date = startDate,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = Math.Round(occupancyRate, 2)
            };
        }
        // Tỷ lệ lấp đầy theo từng ngày trong khoảng thời gian
        public async Task<List<OccupancyDailyStatisticsDto>> GetOccupancyByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;
            if (startDate > endDate)
                throw new BadRequestException("Ngày bắt đầu không được lớn hơn ngày kết thúc!");
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active);
            var bookings = await _context.BookingDetails
                                         .Where(bd => bd.Booking!.IsDeleted == ActiveEnums.Active 
                                             && bd.Booking.Status != BookingEnums.Cancelled
                                             && bd.Booking.Status != BookingEnums.Rejected
                                             && bd.Booking.ExpectedCheckIn < endDate.AddDays(1) 
                                             && bd.Booking.ExpectedCheckOut > startDate)
                                         .Select(bd => new
                                         {
                                            bd.RoomId,
                                            CheckIn = bd.Booking!.ExpectedCheckIn,
                                            CheckOut = bd.Booking.ExpectedCheckOut
                                         })
                                         .ToListAsync();
            var result = new List<OccupancyDailyStatisticsDto>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var nextDate = date.AddDays(1);
                var occupiedRooms = bookings.Where(b => b.CheckIn < nextDate 
                                                     && b.CheckOut > date)
                                            .Select(b => b.RoomId)
                                            .Distinct()
                                            .Count();
                decimal occupancyRate = 0;
                if (totalRooms > 0)
                    occupancyRate = (decimal)occupiedRooms / totalRooms * 100;
                result.Add(new OccupancyDailyStatisticsDto
                {
                    Date = date,
                    TotalRooms = totalRooms,
                    OccupiedRooms = occupiedRooms,
                    OccupancyRate = Math.Round(occupancyRate, 2)
                });
            }
            return result;
        }
        // ===========================================================
        // =              TỶ LỆ LẤP ĐẦY THEO THÁNG                 =
        // ===========================================================

        public async Task<OccupancyMonthlyStatisticsDto> GetOccupancyByMonthAsync(DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1);
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsDeleted == ActiveEnums.Active);
            var bookings = await _context.BookingDetails
                                         .Where(bd => bd.Booking!.IsDeleted == ActiveEnums.Active 
                                             && bd.Booking.Status != BookingEnums.Cancelled 
                                             && bd.Booking.Status != BookingEnums.Rejected
                                             && bd.Booking.ExpectedCheckIn < endDate
                                             && bd.Booking.ExpectedCheckOut > startDate)
                                         .Select(bd => new
                                         {
                                             bd.RoomId,
                                             CheckIn = bd.Booking!.ExpectedCheckIn,
                                             CheckOut = bd.Booking.ExpectedCheckOut
                                         })
                                         .ToListAsync();
            var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            var occupiedRoomNights = 0;
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var nextDate = date.AddDays(1);
                var occupiedRooms = bookings.Where(b => b.CheckIn < nextDate
                                                && b.CheckOut > date)
                                            .Select(b => b.RoomId)
                                            .Distinct()
                                            .Count();
                occupiedRoomNights += occupiedRooms;
            }
            var availableRoomNights = totalRooms * daysInMonth;
            decimal occupancyRate = 0;
            if (availableRoomNights > 0)
                occupancyRate = (decimal)occupiedRoomNights / availableRoomNights * 100;
            return new OccupancyMonthlyStatisticsDto
            {
                Month = startDate,
                TotalRooms = totalRooms,
                OccupiedRoomNights = occupiedRoomNights,
                AvailableRoomNights = availableRoomNights,
                OccupancyRate = Math.Round( occupancyRate, 2)
            };
        }
        // Thống kê tỷ lệ lấp đầy theo từng tháng
        public async Task<List<OccupancyMonthlyStatisticsDto>> GetOccupancyByMonthRangeAsync(DateTime startMonth, DateTime endMonth)
        {
            var currentMonth = new DateTime(startMonth.Year, startMonth.Month, 1);
            var lastMonth = new DateTime(endMonth.Year, endMonth.Month, 1);
            if (currentMonth > lastMonth)
                throw new BadRequestException("Tháng bắt đầu không được lớn hơn tháng kết thúc!");
            var result = new List<OccupancyMonthlyStatisticsDto>();
            while (currentMonth <= lastMonth)
            {
                var statistics = await GetOccupancyByMonthAsync(currentMonth);
                result.Add(statistics);
                currentMonth = currentMonth.AddMonths(1);
            }
            return result;
        }
    }
}
