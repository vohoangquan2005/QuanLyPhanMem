// 404 - NOT FOUND: Không tìm thấy dữ liệu
namespace HotelManagement.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message){ }
    }
}
