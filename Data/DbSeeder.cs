using HotelManagement.Models;
using HotelManagement.Enums;

namespace HotelManagement.Data
{
    public class DbSeeder
    {
        private static DateTime UtcDate(int year, int month, int day) =>
            DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);

        public static void Seed(AppDbContext context)
        {
// =============================================== ROLE ===============================================
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        RoleName = "Admin",
                        Description = "Quản trị hệ thống"
                    },
                    new Role
                    {
                        RoleName = "Staff",
                        Description = "Nhân viên khách sạn"
                    },
                    new Role
                    {
                        RoleName = "Customer",
                        Description = "Khách hàng"
                    }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }
// =============================================== USER ===============================================
            if (!context.Users.Any())
            {
                var adminRole = context.Roles
                    .First(r => r.RoleName == "Admin");
                var staffRole = context.Roles
                    .First(r => r.RoleName == "Staff");
                var customerRole = context.Roles
                    .First(r => r.RoleName == "Customer");
                var users = new List<User>
                {
                    new User
                    {
                        FullName = "Nguyễn Văn Admin",
                        Email = "admin@hotel.com",
                        HashPassword = "123456",
                        RoleId = adminRole.RoleId,
                        IsLocked = ActiveEnums.Active,
                        Gender = "Nam",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        PhoneNumber = "0900000001",
                        Avatar = null,
                        IsDeleted = ActiveEnums.Active
                    },
                    new User
                    {
                        FullName = "Trần Văn Nhân",
                        Email = "staff@hotel.com",
                        HashPassword = "123456",
                        RoleId = staffRole.RoleId,
                        IsLocked = ActiveEnums.Active,
                        Gender = "Nam",
                        DateOfBirth = new DateTime(1995, 5, 10),
                        PhoneNumber = "0900000002",
                        Avatar = null,
                        IsDeleted = ActiveEnums.Active
                    },
                    new User
                    {
                        FullName = "Nguyễn Văn An",
                        Email = "an@gmail.com",
                        HashPassword = "123456",
                        RoleId = customerRole.RoleId,
                        IsLocked = ActiveEnums.Active,
                        Gender = "Nam",
                        DateOfBirth = new DateTime(2000, 3, 15),
                        PhoneNumber = "0900000003",
                        Avatar = null,
                        IsDeleted = ActiveEnums.Active
                    },
                    new User
                    {
                        FullName = "Trần Thị Bình",
                        Email = "binh@gmail.com",
                        HashPassword = "123456",
                        RoleId = customerRole.RoleId,
                        IsLocked = ActiveEnums.Active,
                        Gender = "Nữ",
                        DateOfBirth = new DateTime(1999, 8, 20),
                        PhoneNumber = "0900000004",
                        Avatar = null,
                        IsDeleted = ActiveEnums.Active
                    },
                    new User
                    {
                        FullName = "Lê Minh Cường",
                        Email = "cuong@gmail.com",
                        HashPassword = "123456",
                        RoleId = customerRole.RoleId,
                        IsLocked = ActiveEnums.Active,
                        Gender = "Nam",
                        DateOfBirth = new DateTime(2001, 12, 5),
                        PhoneNumber = "0900000005",
                        Avatar = null,
                        IsDeleted = ActiveEnums.Active
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }
// =============================================== CUSTOMER ===============================================
            if (!context.Customers.Any())
            {
                var userAn = context.Users
                    .First(u => u.Email == "an@gmail.com");
                var userBinh = context.Users
                    .First(u => u.Email == "binh@gmail.com");
                var userCuong = context.Users
                    .First(u => u.Email == "cuong@gmail.com");
                var customers = new List<Customer>
                {
                    new Customer
                    {
                        UserId = userAn.UserId,
                        CCCD = "079200000001",
                        Nationality = "Việt Nam"
                    },
                    new Customer
                    {
                        UserId = userBinh.UserId,
                        CCCD = "079200000002",
                        Nationality = "Việt Nam"
                    },
                    new Customer
                    {
                        UserId = userCuong.UserId,
                        CCCD = "079200000003",
                        Nationality = "Việt Nam"
                    }
                };
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }
// =============================================== ROOM TYPE ===============================================
            if (!context.RoomTypes.Any())
            {
                var roomTypes = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypeName = "Standard",
                        BedCount = 1,
                        BedType = "Giường đơn",
                        Size = "5m x 4m",
                        Description = "Phòng tiêu chuẩn dành cho 1-2 người."
                    },
                    new RoomType
                    {
                        RoomTypeName = "Deluxe",
                        BedCount = 1,
                        BedType = "Giường đôi",
                        Size = "6m x 5m",
                        Description = "Phòng Deluxe rộng rãi, phù hợp cho 2 người."
                    },
                    new RoomType
                    {
                        RoomTypeName = "Family",
                        BedCount = 2,
                        BedType = "2 giường đôi",
                        Size = "8m x 6m",
                        Description = "Phòng gia đình dành cho tối đa 4 người."
                    },
                    new RoomType
                    {
                        RoomTypeName = "Suite",
                        BedCount = 2,
                        BedType = "Giường đôi",
                        Size = "10m x 6m",
                        Description = "Phòng Suite cao cấp với không gian rộng."
                    }
                };
                context.RoomTypes.AddRange(roomTypes);
                context.SaveChanges();
            }
// =============================================== ROOM ===============================================
            if (!context.Rooms.Any())
            {
                var standard = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Standard");
                var deluxe = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Deluxe");
                var family = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Family");
                var suite = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Suite");
                var rooms = new List<Room>
                {
                    // Standard
                    new Room
                    {
                        RoomName = "101",
                        RoomTypeId = standard.RoomTypeId,
                        Capacity = 2,
                        Location = "Tầng 1",
                        PricePerNight = 300000,
                        Description = "Phòng Standard tầng 1.",
                        Status = RoomEnums.Available
                    },
                    new Room
                    {
                        RoomName = "102",
                        RoomTypeId = standard.RoomTypeId,
                        Capacity = 2,
                        Location = "Tầng 1",
                        PricePerNight = 300000,
                        Description = "Phòng Standard tầng 1.",
                        Status = RoomEnums.Available
                    },
                    // Deluxe
                    new Room
                    {
                        RoomName = "201",
                        RoomTypeId = deluxe.RoomTypeId,
                        Capacity = 2,
                        Location = "Tầng 2",
                        PricePerNight = 500000,
                        Description = "Phòng Deluxe tầng 2.",
                        Status = RoomEnums.Available
                    },
                    new Room
                    {
                        RoomName = "202",
                        RoomTypeId = deluxe.RoomTypeId,
                        Capacity = 2,
                        Location = "Tầng 2",
                        PricePerNight = 500000,
                        Description = "Phòng Deluxe tầng 2.",
                        Status = RoomEnums.Available
                    },
                    // Family
                    new Room
                    {
                        RoomName = "301",
                        RoomTypeId = family.RoomTypeId,
                        Capacity = 4,
                        Location = "Tầng 3",
                        PricePerNight = 800000,
                        Description = "Phòng Family dành cho gia đình.",
                        Status = RoomEnums.Available
                    },
                    new Room
                    {
                        RoomName = "302",
                        RoomTypeId = family.RoomTypeId,
                        Capacity = 4,
                        Location = "Tầng 3",
                        PricePerNight = 800000,
                        Description = "Phòng Family dành cho gia đình.",
                        Status = RoomEnums.Available
                    },
                    // Suite
                    new Room
                    {
                        RoomName = "401",
                        RoomTypeId = suite.RoomTypeId,
                        Capacity = 4,
                        Location = "Tầng 4",
                        PricePerNight = 1200000,
                        Description = "Phòng Suite cao cấp.",
                        Status = RoomEnums.Available
                    }
                };

                context.Rooms.AddRange(rooms);
                context.SaveChanges();
            }
// =============================================== ROOM IMAGES ===============================================

// ===============================================SERVICE ===============================================
            if (!context.Services.Any())
            {
                var services = new List<Service>
                {
                    new Service
                    {
                        ServiceName = "Nước suối",
                        UnitPrice = 10000,
                        Unit = "Chai",
                        Description = "Nước suối đóng chai.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new Service
                    {
                        ServiceName = "Nước ngọt",
                        UnitPrice = 15000,
                        Unit = "Lon",
                        Description = "Các loại nước ngọt đóng lon.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new Service
                    {
                        ServiceName = "Giặt ủi",
                        UnitPrice = 50000,
                        Unit = "Kg",
                        Description = "Dịch vụ giặt và ủi quần áo.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new Service
                    {
                        ServiceName = "Ăn sáng",
                        UnitPrice = 80000,
                        Unit = "Suất",
                        Description = "Suất ăn sáng tại khách sạn.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new Service
                    {
                        ServiceName = "Thuê xe máy",
                        UnitPrice = 150000,
                        Unit = "Ngày",
                        Description = "Dịch vụ cho thuê xe máy.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new Service
                    {
                        ServiceName = "Đưa đón sân bay",
                        UnitPrice = 300000,
                        Unit = "Chuyến",
                        Description = "Dịch vụ đưa đón khách từ sân bay.",
                        IsDeleted = ActiveEnums.Active
                    }
                };
                context.Services.AddRange(services);
                context.SaveChanges();
            }
// =============================================== SERVICE PACKAGE ===============================================
            if (!context.ServicePackages.Any())
            {
                var servicePackages = new List<ServicePackage>
                {
                    new ServicePackage
                    {
                        PackageName = "Gói ăn sáng",
                        PackagePrice = 150000,
                        Description = "Bao gồm bữa sáng cho khách trong thời gian lưu trú.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new ServicePackage
                    {
                        PackageName = "Gói tiện ích",
                        PackagePrice = 250000,
                        Description = "Bao gồm nước suối, nước ngọt và ăn sáng.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new ServicePackage
                    {
                        PackageName = "Gói gia đình",
                        PackagePrice = 400000,
                        Description = "Bao gồm ăn sáng, nước uống và một số tiện ích dành cho gia đình.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new ServicePackage
                    {
                        PackageName = "Gói cao cấp",
                        PackagePrice = 600000,
                        Description = "Bao gồm ăn sáng, nước uống, giặt ủi và dịch vụ đưa đón.",
                        IsDeleted = ActiveEnums.Active
                    }
                };
                context.ServicePackages.AddRange(servicePackages);
                context.SaveChanges();
            }
// =============================================== PROMOTION ===============================================
            if (!context.Promotions.Any())
            {
                var promotions = new List<Promotion>
                {
                    new Promotion
                    {
                        PromotionCode = "WELCOME10",
                        PromotionName = "Giảm 10% cho khách hàng mới",
                        DiscountType = PromotionDiscountTypeEnums.Percentage,
                        DiscountValue = 10m,
                        MinAmount = 500000m,
                        MaxDisCountAmount = 200000m,
                        UsageLimit = 100,
                        UsedCount = 0,
                        StartDate = UtcDate(2026, 1, 1),
                        EndDate = UtcDate(2027, 1, 1)
                    },
                    new Promotion
                    {
                        PromotionCode = "SUMMER15",
                        PromotionName = "Ưu đãi mùa hè",
                        DiscountType = PromotionDiscountTypeEnums.Percentage,
                        DiscountValue = 15m,
                        MinAmount = 1000000m,
                        MaxDisCountAmount = 300000m,
                        UsageLimit = 50,
                        UsedCount = 0,
                        StartDate = UtcDate(2026, 5, 1),
                        EndDate = UtcDate(2026, 10, 1)
                    },
                    new Promotion
                    {
                        PromotionCode = "VIP20",
                        PromotionName = "Ưu đãi khách hàng VIP",
                        DiscountType = PromotionDiscountTypeEnums.Percentage,
                        DiscountValue = 20m,
                        MinAmount = 2000000m,
                        MaxDisCountAmount = 500000m,
                        UsageLimit = 30,
                        UsedCount = 0,
                        StartDate = UtcDate(2026, 1, 1),
                        EndDate = UtcDate(2027, 1, 1)
                    }
                };
                context.Promotions.AddRange(promotions);
                context.SaveChanges();
            }
// =============================================== APPLY PROMOTION ===============================================
            if (!context.ApplyPromotions.Any())
            {
                var welcome10 = context.Promotions
                    .First(p => p.PromotionCode == "WELCOME10");
                var summer15 = context.Promotions
                    .First(p => p.PromotionCode == "SUMMER15");
                var vip20 = context.Promotions
                    .First(p => p.PromotionCode == "VIP20");
                var standard = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Standard");
                var deluxe = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Deluxe");
                var family = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Family");
                var suite = context.RoomTypes
                    .First(rt => rt.RoomTypeName == "Suite");

                var applyPromotions = new List<ApplyPromotion>
                {
                    // WELCOME10
                    new ApplyPromotion
                    {
                        PromotionId = welcome10.PromotionId,
                        RoomTypeId = standard.RoomTypeId
                    },
                    new ApplyPromotion
                    {
                        PromotionId = welcome10.PromotionId,
                        RoomTypeId = deluxe.RoomTypeId
                    },
                    new ApplyPromotion
                    {
                        PromotionId = welcome10.PromotionId,
                        RoomTypeId = family.RoomTypeId
                    },
                    // SUMMER15
                    new ApplyPromotion
                    {
                        PromotionId = summer15.PromotionId,
                        RoomTypeId = deluxe.RoomTypeId
                    },
                    new ApplyPromotion
                    {
                        PromotionId = summer15.PromotionId,
                        RoomTypeId = family.RoomTypeId
                    },
                    new ApplyPromotion
                    {
                        PromotionId = summer15.PromotionId,
                        RoomTypeId = suite.RoomTypeId
                    },
                    // VIP20
                    new ApplyPromotion
                    {
                        PromotionId = vip20.PromotionId,
                        RoomTypeId = family.RoomTypeId
                    },
                    new ApplyPromotion
                    {
                        PromotionId = vip20.PromotionId,
                        RoomTypeId = suite.RoomTypeId
                    }
                };
                context.ApplyPromotions.AddRange(applyPromotions);
                context.SaveChanges();
            }

// =============================================== CANCELLATION POLICY ===============================================
            if (!context.CancellationPolicies.Any())
            {
                var policies = new List<CancellationPolicy>
                {
                    new CancellationPolicy
                    {
                        PolicyName = "Hủy trước 7 ngày",
                        CancellationTime = 7,
                        CancellationTimeUnit = CancellationTimeUnitEnums.Day,
                        RefundPercentage = 100m,
                        Description = "Hủy trong 7 ngày từ thời điểm đặt phòng được hoàn 50% tiền cọc.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new CancellationPolicy
                    {
                        PolicyName = "Hủy trước 3 ngày",
                        CancellationTime = 3,
                        CancellationTimeUnit = CancellationTimeUnitEnums.Day,
                        RefundPercentage = 70m,
                        Description = "Hủy trong 3 ngày từ thời điểm đặt phòng được hoàn 90% tiền cọc.",
                        IsDeleted = ActiveEnums.Active
                    },
                    new CancellationPolicy
                    {
                        PolicyName = "Hủy trước 24 giờ",
                        CancellationTime = 24,
                        CancellationTimeUnit = CancellationTimeUnitEnums.Hour,
                        RefundPercentage = 100m,
                        Description = "Hủy trong vòng 24 giờ từ thời điểm đặt phòng được hoàn 100% tiền cọc.",
                        IsDeleted = ActiveEnums.Active
                    },
                };
                context.CancellationPolicies.AddRange(policies);
                context.SaveChanges();
            }
// =============================================== BOOKING ===============================================
            if (!context.Bookings.Any())
            {
                var customerAn = context.Customers
                    .First(c => c.User!.Email == "an@gmail.com");
                var customerBinh = context.Customers
                    .First(c => c.User!.Email == "binh@gmail.com");
                var customerCuong = context.Customers
                    .First(c => c.User!.Email == "cuong@gmail.com");
                var welcome10 = context.Promotions
                    .First(p => p.PromotionCode == "WELCOME10");
                var summer15 = context.Promotions
                    .First(p => p.PromotionCode == "SUMMER15");
                var vip20 = context.Promotions
                    .First(p => p.PromotionCode == "VIP20");
                var breakfastPackage = context.ServicePackages
                    .First(p => p.PackageName == "Gói ăn sáng");
                var familyPackage = context.ServicePackages
                    .First(p => p.PackageName == "Gói gia đình");
                var bookings = new List<Booking>
                {
                    // Booking hoàn thành
                    new Booking
                    {
                        CustomerId = customerAn.CustomerId,
                        BookingDate = new DateTime(2026, 8, 1),
                        ExpectedCheckIn = new DateTime(2026, 8, 10),
                        ExpectedCheckOut = new DateTime(2026, 8, 12),
                        GuestCount = 2,
                        RoomCount = 1,
                        PromotionId = welcome10.PromotionId,
                        PromotionNameSnapshot = welcome10.PromotionName,
                        PromotionDiscountTypeSnapshot = welcome10.DiscountType,
                        PromotionDiscountValueSnapshot = welcome10.DiscountValue,
                        ServicePackageId = breakfastPackage.ServicePackageId,
                        DepositAmount = 200000m,
                        RoomAmount = 600000m,
                        PackageAmount = 150000m,
                        DiscountAmount = 60000m,
                        AdditionalServiceAmount = 100000m,
                        TotalAmount = 790000m,
                        Note = "Khách đã hoàn thành kỳ lưu trú.",
                        Status = BookingEnums.Completed
                    },
                    // Booking đã xác nhận
                    new Booking
                    {
                        CustomerId = customerBinh.CustomerId,
                        BookingDate = new DateTime(2026, 9, 1),
                        ExpectedCheckIn = new DateTime(2026, 10, 5),
                        ExpectedCheckOut = new DateTime(2026, 10, 8),
                        GuestCount = 3,
                        RoomCount = 2,
                        PromotionId = summer15.PromotionId,
                        PromotionNameSnapshot = summer15.PromotionName,
                        PromotionDiscountTypeSnapshot = summer15.DiscountType,
                        PromotionDiscountValueSnapshot = summer15.DiscountValue,
                        ServicePackageId = familyPackage.ServicePackageId,
                        DepositAmount = 800000m,
                        RoomAmount = 3900000m,
                        PackageAmount = 400000m,
                        DiscountAmount = 300000m,
                        AdditionalServiceAmount = 0m,
                        TotalAmount = 4000000m,
                        Note = "Khách đặt 2 phòng cho gia đình.",
                        Status = BookingEnums.Confirmed
                    },
                    // Booking đang xử lý
                    new Booking
                    {
                        CustomerId = customerCuong.CustomerId,
                        BookingDate = new DateTime(2026, 9, 15),
                        ExpectedCheckIn = new DateTime(2026, 10, 15),
                        ExpectedCheckOut = new DateTime(2026, 10, 17),
                        GuestCount = 2,
                        RoomCount = 1,
                        PromotionId = summer15.PromotionId,
                        PromotionNameSnapshot = summer15.PromotionName,
                        PromotionDiscountTypeSnapshot = summer15.DiscountType,
                        PromotionDiscountValueSnapshot = summer15.DiscountValue,
                        ServicePackageId = null,
                        DepositAmount = 170000m,
                        RoomAmount = 1000000m,
                        PackageAmount = 0m,
                        DiscountAmount = 150000m,
                        AdditionalServiceAmount = 0m,
                        TotalAmount = 850000m,
                        Note = "Khách đang chờ xác nhận.",
                        Status = BookingEnums.Pending
                    },
                    // Booking đã nhận phòng
                    new Booking
                    {
                        CustomerId = customerAn.CustomerId,
                        BookingDate = new DateTime(2026, 9, 10),
                        ExpectedCheckIn = new DateTime(2026, 9, 20),
                        ExpectedCheckOut = new DateTime(2026, 9, 23),
                        GuestCount = 4,
                        RoomCount = 1,
                        PromotionId = summer15.PromotionId,
                        PromotionNameSnapshot = summer15.PromotionName,
                        PromotionDiscountTypeSnapshot = summer15.DiscountType,
                        PromotionDiscountValueSnapshot = summer15.DiscountValue,
                        ServicePackageId = familyPackage.ServicePackageId,
                        DepositAmount = 500000m,
                        RoomAmount = 2400000m,
                        PackageAmount = 400000m,
                        DiscountAmount = 300000m,
                        AdditionalServiceAmount = 400000m,
                        TotalAmount = 2900000m,
                        Note = "Khách đang lưu trú.",
                        Status = BookingEnums.CheckedIn
                    },
                    // Booking đã hủy
                    new Booking
                    {
                        CustomerId = customerBinh.CustomerId,
                        BookingDate = new DateTime(2026, 9, 1),
                        ExpectedCheckIn = new DateTime(2026, 9, 25),
                        ExpectedCheckOut = new DateTime(2026, 9, 27),
                        GuestCount = 2,
                        RoomCount = 1,
                        PromotionId = summer15.PromotionId,
                        PromotionNameSnapshot = summer15.PromotionName,
                        PromotionDiscountTypeSnapshot = summer15.DiscountType,
                        PromotionDiscountValueSnapshot = summer15.DiscountValue,
                        ServicePackageId = null,
                        DepositAmount = 420000m,
                        RoomAmount = 2400000m,
                        PackageAmount = 0m,
                        DiscountAmount = 300000m,
                        AdditionalServiceAmount = 0m,
                        TotalAmount = 2100000m,
                        Note = "Khách yêu cầu hủy đặt phòng.",
                        Status = BookingEnums.Cancelled
                    }
                };
                context.Bookings.AddRange(bookings);
                context.SaveChanges();

                welcome10.UsedCount = bookings.Count(b => b.PromotionId == welcome10.PromotionId);
                summer15.UsedCount = bookings.Count(b => b.PromotionId == summer15.PromotionId);
                vip20.UsedCount = bookings.Count(b => b.PromotionId == vip20.PromotionId);
                context.SaveChanges();
            }
// =============================================== BOOKING DETAILS ===============================================
            if (!context.BookingDetails.Any())
            {
                var booking1 = context.Bookings.First(b =>
                    b.Customer!.User!.Email == "an@gmail.com" &&
                    b.Status == BookingEnums.Completed);
                var booking2 = context.Bookings.First(b =>
                    b.Customer!.User!.Email == "binh@gmail.com" &&
                    b.Status == BookingEnums.Confirmed);
                var booking3 = context.Bookings.First(b =>
                    b.Customer!.User!.Email == "cuong@gmail.com" &&
                    b.Status == BookingEnums.Pending);
                var booking4 = context.Bookings.First(b =>
                    b.Customer!.User!.Email == "an@gmail.com" &&
                    b.Status == BookingEnums.CheckedIn);
                var booking5 = context.Bookings.First(b =>
                    b.Customer!.User!.Email == "binh@gmail.com" &&
                    b.Status == BookingEnums.Cancelled);
                var room101 = context.Rooms.First(r => r.RoomName == "101");
                var room201 = context.Rooms.First(r => r.RoomName == "201");
                var room202 = context.Rooms.First(r => r.RoomName == "202");
                var room301 = context.Rooms.First(r => r.RoomName == "301");
                var room401 = context.Rooms.First(r => r.RoomName == "401");
                var room102 = context.Rooms.First(r => r.RoomName == "102");
                var bookingDetails = new List<BookingDetail>
                {
                    // Booking 1 - Completed
                    new BookingDetail
                    {
                        BookingId = booking1.BookingId,
                        RoomId = room101.RoomId,
                        ActualCheckIn = new DateTime(2026, 8, 10, 14, 0, 0),
                        ActualCheckOut = new DateTime(2026, 8, 12, 12, 0, 0),
                        PricePerNight = 300000m,
                        TotalAmount = 600000m,
                        GuestCount = 2
                    },
                    // Booking 2 - 2 phòng
                    new BookingDetail
                    {
                        BookingId = booking2.BookingId,
                        RoomId = room201.RoomId,
                        ActualCheckIn = null,
                        ActualCheckOut = null,
                        PricePerNight = 500000m,
                        TotalAmount = 1500000m,
                        GuestCount = 2
                    },
                    new BookingDetail
                    {
                        BookingId = booking2.BookingId,
                        RoomId = room301.RoomId,
                        ActualCheckIn = null,
                        ActualCheckOut = null,
                        PricePerNight = 800000m,
                        TotalAmount = 2400000m,
                        GuestCount = 1
                    },
                    // Booking 3 - Pending
                    new BookingDetail
                    {
                        BookingId = booking3.BookingId,
                        RoomId = room202.RoomId,
                        ActualCheckIn = null,
                        ActualCheckOut = null,
                        PricePerNight = 500000m,
                        TotalAmount = 1000000m,
                        GuestCount = 2
                    },
                    // Booking 4 - CheckedIn
                    new BookingDetail
                    {
                        BookingId = booking4.BookingId,
                        RoomId = room301.RoomId,
                        ActualCheckIn = new DateTime(2026, 9, 20, 14, 0, 0),
                        ActualCheckOut = null,
                        PricePerNight = 800000m,
                        TotalAmount = 2400000m,
                        GuestCount = 4
                    },
                    // Booking 5 - Cancelled
                    new BookingDetail
                    {
                        BookingId = booking5.BookingId,
                        RoomId = room401.RoomId,
                        ActualCheckIn = null,
                        ActualCheckOut = null,
                        PricePerNight = 1200000m,
                        TotalAmount = 2400000m,
                        GuestCount = 2
                    }
                };
                context.BookingDetails.AddRange(bookingDetails);
                context.SaveChanges();
            }
// =============================================== GUEST ===============================================
            if (!context.Guests.Any())
            {
                var bookingDetail1 = context.BookingDetails.First(bd =>
                    bd.Booking!.Customer!.User!.Email == "an@gmail.com" &&
                    bd.Booking.Status == BookingEnums.Completed &&
                    bd.Room!.RoomName == "101");
                var bookingDetail2 = context.BookingDetails.First(bd =>
                    bd.Booking!.Customer!.User!.Email == "binh@gmail.com" &&
                    bd.Booking.Status == BookingEnums.Confirmed &&
                    bd.Room!.RoomName == "201");
                var bookingDetail3 = context.BookingDetails.First(bd =>
                    bd.Booking!.Customer!.User!.Email == "binh@gmail.com" &&
                    bd.Booking.Status == BookingEnums.Confirmed &&
                    bd.Room!.RoomName == "301");
                var bookingDetail4 = context.BookingDetails.First(bd =>
                    bd.Booking!.Customer!.User!.Email == "an@gmail.com" &&
                    bd.Booking.Status == BookingEnums.CheckedIn);
                var guests = new List<Guest>
                {
                    // BookingDetail 1
                    new Guest
                    {
                        BookingDetailId = bookingDetail1.BookingDetailId,
                        FullName = "Nguyễn Văn An",
                        DateOfBirth = new DateTime(2000, 3, 15),
                        Gender = "Nam",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000003",
                        Email = "an@gmail.com",
                        Note = "Khách chính"
                    },
                    new Guest
                    {
                        BookingDetailId = bookingDetail1.BookingDetailId,
                        FullName = "Nguyễn Thị Hoa",
                        DateOfBirth = new DateTime(2001, 6, 20),
                        Gender = "Nữ",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000010",
                        Email = "hoa@gmail.com"
                    },
                    // BookingDetail 2
                    new Guest
                    {
                        BookingDetailId = bookingDetail2.BookingDetailId,
                        FullName = "Trần Thị Bình",
                        DateOfBirth = new DateTime(1999, 8, 20),
                        Gender = "Nữ",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000004",
                        Email = "binh@gmail.com",
                        Note = "Khách chính"
                    },
                    new Guest
                    {
                        BookingDetailId = bookingDetail2.BookingDetailId,
                        FullName = "Trần Văn Nam",
                        DateOfBirth = new DateTime(1998, 4, 12),
                        Gender = "Nam",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000011",
                        Email = "nam@gmail.com"
                    },
                    // BookingDetail 3
                    new Guest
                    {
                        BookingDetailId = bookingDetail3.BookingDetailId,
                        FullName = "Lê Văn Minh",
                        DateOfBirth = new DateTime(2005, 10, 10),
                        Gender = "Nam",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000012",
                        Email = "minh@gmail.com"
                    },
                    // BookingDetail 4
                    new Guest
                    {
                        BookingDetailId = bookingDetail4.BookingDetailId,
                        FullName = "Nguyễn Văn An",
                        DateOfBirth = new DateTime(2000, 3, 15),
                        Gender = "Nam",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000003",
                        Email = "an@gmail.com"
                    },
                    new Guest
                    {
                        BookingDetailId = bookingDetail4.BookingDetailId,
                        FullName = "Nguyễn Thị Lan",
                        DateOfBirth = new DateTime(1997, 11, 8),
                        Gender = "Nữ",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000013",
                        Email = "lan@gmail.com"
                    },
                    new Guest
                    {
                        BookingDetailId = bookingDetail4.BookingDetailId,
                        FullName = "Nguyễn Minh Khang",
                        DateOfBirth = new DateTime(1995, 2, 18),
                        Gender = "Nam",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000014",
                        Email = "khang@gmail.com"
                    },
                    new Guest
                    {
                        BookingDetailId = bookingDetail4.BookingDetailId,
                        FullName = "Nguyễn Minh Anh",
                        DateOfBirth = new DateTime(2002, 7, 25),
                        Gender = "Nữ",
                        Nationality = "Việt Nam",
                        PhoneNumber = "0900000015",
                        Email = "anh@gmail.com"
                    }
                };
                context.Guests.AddRange(guests);
                context.SaveChanges();
            }
// =============================================== BOOKING SERVICES ===============================================
            if (!context.BookingServices.Any())
            {
                var bookingCompleted = context.Bookings.First(b =>
                    b.Status == BookingEnums.Completed);
                var bookingCheckedIn = context.Bookings.First(b =>
                    b.Status == BookingEnums.CheckedIn);
                var water = context.Services.First(s => s.ServiceName == "Nước suối");
                var breakfast = context.Services.First(s => s.ServiceName == "Ăn sáng");
                var laundry = context.Services.First(s => s.ServiceName == "Giặt ủi");
                var airport = context.Services.First(s => s.ServiceName == "Đưa đón sân bay");
                var bookingServices = new List<BookingService>
                {
                    new BookingService
                    {
                        BookingId = bookingCompleted.BookingId,
                        ServiceId = water.ServiceId,
                        Quantity = 2,
                        UnitPrice = 10000m,
                        TotalAmount = 20000m
                    },
                    new BookingService
                    {
                        BookingId = bookingCompleted.BookingId,
                        ServiceId = breakfast.ServiceId,
                        Quantity = 1,
                        UnitPrice = 80000m,
                        TotalAmount = 80000m
                    },
                    new BookingService
                    {
                        BookingId = bookingCheckedIn.BookingId,
                        ServiceId = laundry.ServiceId,
                        Quantity = 2,
                        UnitPrice = 50000m,
                        TotalAmount = 100000m
                    },
                    new BookingService
                    {
                        BookingId = bookingCheckedIn.BookingId,
                        ServiceId = airport.ServiceId,
                        Quantity = 1,
                        UnitPrice = 300000m,
                        TotalAmount = 300000m
                    }
                };
                context.BookingServices.AddRange(bookingServices);
                context.SaveChanges();
            }
// =============================================== INVOICES ===============================================
            if (!context.Invoices.Any())
            {
                var admin = context.Users
                    .First(u => u.Email == "admin@hotel.com");
                var bookingCompleted = context.Bookings
                    .First(b => b.Status == BookingEnums.Completed);
                var bookingConfirmed = context.Bookings
                    .First(b => b.Status == BookingEnums.Confirmed);
                var bookingCheckedIn = context.Bookings
                    .First(b => b.Status == BookingEnums.CheckedIn);
                var invoices = new List<Invoice>
                {
                    // Đã thanh toán
                    new Invoice
                    {
                        BookingId = bookingCompleted.BookingId,
                        SubTotal = 890000m,
                        TaxAmount = 83000m,
                        DiscountAmount = 60000m,
                        AmountDue = 913000m,
                        Status = InvoiceEnums.Paid,
                        CreatedByUserId = admin.UserId
                    },
                    // Thanh toán một phần
                    new Invoice
                    {
                        BookingId = bookingConfirmed.BookingId,
                        SubTotal = 4300000m,
                        TaxAmount = 400000m,
                        DiscountAmount = 300000m,
                        AmountDue = 4400000m,
                        Status = InvoiceEnums.PartiallyPaid,
                        CreatedByUserId = admin.UserId
                    },
                    new Invoice
                    {
                        BookingId = bookingCheckedIn.BookingId,
                        SubTotal = 3200000m,
                        TaxAmount = 290000m,
                        DiscountAmount = 300000m,
                        AmountDue = 3190000m,
                        Status = InvoiceEnums.Unpaid,
                        CreatedByUserId = admin.UserId
                    }
                };
                context.Invoices.AddRange(invoices);
                context.SaveChanges();
            }
// =============================================== PAYMENTS ===============================================
            if (!context.Payments.Any())
            {
                var admin = context.Users
                    .First(u => u.Email == "admin@hotel.com");
                var invoicePaid = context.Invoices
                    .First(i => i.Status == InvoiceEnums.Paid);
                var invoicePartiallyPaid = context.Invoices
                    .First(i => i.Status == InvoiceEnums.PartiallyPaid);
                var payments = new List<Payment>
                {
                    // Tiền cọc
                    new Payment
                    {
                        InvoiceId = invoicePaid.InvoiceId,
                        Amount = 200000m,
                        PaymentType = PaymentTypeEnums.Deposit,
                        PaymentMethod = "Chuyển khoản",
                        TransactionCode = "TXN-20260801-001",
                        Status = PaymentEnums.Success,
                        CreatedByUserId = admin.UserId,
                        PaymentDate = new DateTime(2026, 8, 1)
                    },
                    // Thanh toán lần cuối
                    new Payment
                    {
                        InvoiceId = invoicePaid.InvoiceId,
                        Amount = 713000m,
                        PaymentType = PaymentTypeEnums.Payment,
                        PaymentMethod = "Tiền mặt",
                        TransactionCode = "TXN-20260812-001",
                        Status = PaymentEnums.Success,
                        CreatedByUserId = admin.UserId,
                        PaymentDate = new DateTime(2026, 8, 12)
                    },
                    // Cọc cho booking xác nhận
                    new Payment
                    {
                        InvoiceId = invoicePartiallyPaid.InvoiceId,
                        Amount = 500000m,
                        PaymentType = PaymentTypeEnums.Deposit,
                        PaymentMethod = "Chuyển khoản",
                        TransactionCode = "TXN-20260901-001",
                        Status = PaymentEnums.Success,
                        CreatedByUserId = admin.UserId,
                        PaymentDate = new DateTime(2026, 9, 1)
                    },
                    // Thanh toán lần cuối chưa thành công
                    new Payment
                    {
                        InvoiceId = invoicePartiallyPaid.InvoiceId,
                        Amount = 1000000m,
                        PaymentType = PaymentTypeEnums.Payment,
                        PaymentMethod = "Chuyển khoản",
                        TransactionCode = "TXN-20260902-001",
                        Status = PaymentEnums.Failed,
                        CreatedByUserId = admin.UserId,
                        PaymentDate = new DateTime(2026, 9, 2)
                    }
                };
                context.Payments.AddRange(payments);
                context.SaveChanges();
            }
// =============================================== CANCELLATIONS ===============================================
            if (!context.Cancellations.Any())
            {
                var cancelledBooking = context.Bookings
                    .First(b => b.Status == BookingEnums.Cancelled);
                var policy = context.CancellationPolicies
                    .First(p => p.PolicyName == "Hủy trước 3 ngày");
                var cancellation = new Cancellation
                {
                    BookingId = cancelledBooking.BookingId,
                    CancellationPolicyId = policy.CancellationPolicyId,
                    Reason = "Khách có việc đột xuất và yêu cầu hủy phòng.",
                    RefundPercentage = 70m,
                    RefundAmount = 294000m,
                    Status = CancellationEnums.Approved
                };
                context.Cancellations.Add(cancellation);
                context.SaveChanges();
            }
// =============================================== REVIEWS ===============================================
            if (!context.Reviews.Any())
            {
                var customer = context.Customers
                    .First(c => c.User!.Email == "an@gmail.com");

                var bookingCompleted = context.Bookings
                    .First(b =>
                        b.Customer!.User!.Email == "an@gmail.com" &&
                        b.Status == BookingEnums.Completed);

                var room101 = context.Rooms
                    .First(r => r.RoomName == "101");

                var room102 = context.Rooms
                    .First(r => r.RoomName == "102");

                var reviews = new List<Review>
                {
                    new Review
                    {
                        CustomerId = customer.CustomerId,
                        RoomId = room101.RoomId,
                        BookingId = bookingCompleted.BookingId,
                        Rating = 5,
                        Comment = "Phòng sạch sẽ, nhân viên thân thiện.",
                        Status = ReviewEnums.Approved
                    },

                    new Review
                    {
                        CustomerId = customer.CustomerId,
                        RoomId = room102.RoomId,
                        BookingId = bookingCompleted.BookingId,
                        Rating = 2,
                        Comment = "Một số thiết bị trong phòng cần được kiểm tra.",
                        Status = ReviewEnums.Rejected
                    }
                };

                context.Reviews.AddRange(reviews);
                context.SaveChanges();
            }
// =============================================== FAVORITE ROOMS ===============================================
            if (!context.FavoriteRooms.Any())
            {
                var customerAn = context.Customers
                    .First(c => c.User!.Email == "an@gmail.com");
                var customerBinh = context.Customers
                    .First(c => c.User!.Email == "binh@gmail.com");
                var customerCuong = context.Customers
                    .First(c => c.User!.Email == "cuong@gmail.com");
                var room201 = context.Rooms.First(r => r.RoomName == "201");
                var room301 = context.Rooms.First(r => r.RoomName == "301");
                var room401 = context.Rooms.First(r => r.RoomName == "401");
                var room102 = context.Rooms.First(r => r.RoomName == "102");
                var favoriteRooms = new List<FavoriteRoom>
                {
                    new FavoriteRoom
                    {
                        CustomerId = customerAn.CustomerId,
                        RoomId = room201.RoomId
                    },
                    new FavoriteRoom
                    {
                        CustomerId = customerAn.CustomerId,
                        RoomId = room301.RoomId
                    },
                    new FavoriteRoom
                    {
                        CustomerId = customerBinh.CustomerId,
                        RoomId = room401.RoomId
                    },
                    new FavoriteRoom
                    {
                        CustomerId = customerCuong.CustomerId,
                        RoomId = room102.RoomId
                    }
                };
                context.FavoriteRooms.AddRange(favoriteRooms);
                context.SaveChanges();
            }
// =============================================== AUDIT LOGS ===============================================
            if (!context.AuditLogs.Any())
            {
                var admin = context.Users
                    .First(u => u.Email == "admin@hotel.com");
                var staff = context.Users
                    .First(u => u.Email == "staff@hotel.com");
                var auditLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        UserId = admin.UserId,
                        Action = AuditLogEnums.Login,
                        TableName = "Users",
                        RecordId = admin.UserId,
                        OldValue = null,
                        NewValue = "Đăng nhập hệ thống"
                    },
                    new AuditLog
                    {
                        UserId = staff.UserId,
                        Action = AuditLogEnums.Create ,
                        TableName = "Bookings",
                        RecordId = 1,
                        OldValue = null,
                        NewValue = "{\"Status\":\"Pending\",\"GuestCount\":2,\"RoomCount\":1}"
                    },
                    new AuditLog
                    {
                        UserId = staff.UserId,
                        Action = AuditLogEnums.Update,
                        TableName = "Bookings",
                        RecordId = 2,
                        OldValue = "{\"Status\":\"Pending\"}",
                        NewValue = "{\"Status\":\"Confirmed\"}"
                    },
                    new AuditLog
                    {
                        UserId = admin.UserId,
                        Action = AuditLogEnums.Update,
                        TableName = "Rooms",
                        RecordId = 3,
                        OldValue = "{\"Status\":\"Maintenance\"}",
                        NewValue = "{\"Status\":\"Available\"}"
                    },
                    new AuditLog
                    {
                        UserId = staff.UserId,
                        Action = AuditLogEnums.SoftDelete,
                        TableName = "ServicePackages",
                        RecordId = 2,
                        OldValue = "{\"IsDeleted\":\"Active\"}",
                        NewValue = "{\"IsDeleted\":\"Inactive\"}"
                    },
                    new AuditLog
                    {
                        UserId = admin.UserId,
                        Action = AuditLogEnums.Logout,
                        TableName = "Users",
                        RecordId = admin.UserId,
                        OldValue = null,
                        NewValue = "Đăng xuất hệ thống"
                    }
                };
                context.AuditLogs.AddRange(auditLogs);
                context.SaveChanges();
            }
        }
    }
}
