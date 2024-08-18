using Huojian.LibraryManagement.Components.Protocol.Swager;
using Huojian.LibraryManagement.Components.Protocol.Swager.Model;

namespace Huojian.LibraryManagement.Components.Swagger
{
    public class GlobalClient:IGlobalClient
    {
        public Task<bool> AddUser(UserInfo info)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveUser(UserInfo info)
        {
            throw new NotImplementedException();
        }

        public Task<UserInfo> UpdateUserInfo(UserInfo newUserInfo)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserInfo>> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<UserInfo> GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserInfo> LogIn(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddCategory(BookCategory category)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveCategory(BookCategory category)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCategory(BookCategory category)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BookCategory>> GetCategories()
        {
            throw new NotImplementedException();
        }

        public Task<BookCategory> GetCategoryById(string categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddBook(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddBooks(IEnumerable<Book> books)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveBook(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<Book> UpdateBook(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Book>> GetBooks()
        {
            throw new NotImplementedException();
        }

        public Task<Book> GetBookById(string isbn)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddRecord(BorrowRecord record)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRecord(BorrowRecord record)
        {
            throw new NotImplementedException();
        }

        public Task<BorrowRecord> UpdateRecord(BorrowRecord record)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BorrowRecord>> GetRecordsByReaderId(string readerId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BorrowRecord>> GetRecordsByBookId(string bookId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Book>> SearchBookByName(string bookName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Book>> SearchBookByAuthorName(string AuthorName)
        {
            throw new NotImplementedException();
        }
    }
}
