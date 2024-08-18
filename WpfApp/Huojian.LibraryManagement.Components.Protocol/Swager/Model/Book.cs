namespace Huojian.LibraryManagement.Components.Protocol.Swager.Model
{
    public class Book
    {
        public string ISBN { get; set; }

        public BookCategory Category { get; set; }

        public string AuthorName { get; set; }

        public double Price { get; set; }

        public BookStatus Status { get; set; }
    }

    public enum BookStatus
    {
        Free,
        Borrowed,
    }
}