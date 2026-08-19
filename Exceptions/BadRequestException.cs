// 400 - BAD REQUEST: Request không hợp lệ
namespace HotelManagement.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}