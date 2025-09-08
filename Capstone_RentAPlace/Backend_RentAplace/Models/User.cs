namespace RentAPlaceAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Property>? Properties { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
