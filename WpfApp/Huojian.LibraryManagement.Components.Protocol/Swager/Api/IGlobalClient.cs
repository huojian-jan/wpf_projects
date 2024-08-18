using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Huojian.LibraryManagement.Components.Protocol.Swager.Model;

namespace Huojian.LibraryManagement.Components.Protocol.Swager
{
    public interface IGlobalClient
    {
       public Task<bool> AddUser(UserInfo info);
        public Task<bool> RemoveUser(UserInfo info);

        public Task<UserInfo> UpdateUserInfo(UserInfo newUserInfo);

        public Task<IEnumerable<UserInfo>> GetUsers();

        public Task<UserInfo> GetUserById(string userId);

        public Task<UserInfo> LogIn(string userName,string password);

        public Task<bool>  AddCategory(BookCategory category);

        public Task<bool> RemoveCategory(BookCategory category);

        public Task<bool> UpdateCategory(BookCategory category);

        public Task<IEnumerable<BookCategory>> GetCategories();

        public Task<BookCategory> GetCategoryById(string categoryId);

        public Task<bool> AddBook(Book book);

        public Task<bool> AddBooks(IEnumerable<Book> books);

        public Task<bool> RemoveBook(Book book);

        public Task<Book> UpdateBook(Book book);

        public Task<IEnumerable<Book>> GetBooks();

        public Task<Book> GetBookById(string isbn);

        public Task<bool> AddRecord(BorrowRecord record);

        public Task<bool> RemoveRecord(BorrowRecord record);

        public Task<BorrowRecord> UpdateRecord(BorrowRecord record);

        public Task<IEnumerable<BorrowRecord>> GetRecordsByReaderId(string readerId);

        public Task<IEnumerable<BorrowRecord>> GetRecordsByBookId(string bookId);

        public Task<IEnumerable<Book>> SearchBookByName(string bookName);

        public Task<IEnumerable<Book>> SearchBookByAuthorName(string AuthorName);
    }
}
