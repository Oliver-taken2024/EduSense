namespace EduSense.DAL.Models
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public required string UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
