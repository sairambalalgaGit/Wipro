namespace LibraryEFCore.Models
{
    public class Author
    {
        public int AuthorID { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }

        // Navigation
        public ICollection<Book> Books { get; set; }
    }
}
