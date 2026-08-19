// 409 - CONFLICT: Đã tồn tại dữ liệu
namespace HotelManagement.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message){ }
    }
}
