namespace LibraryEFCore.Models
{
        public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }

        // Foreign Key
        public int AuthorID { get; set; }
        public Author Author { get; set; }

       
    }
}
