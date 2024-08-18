using SQLite;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using Microsoft.VisualBasic;
using ColumnAttribute = SQLite.ColumnAttribute;
using SQLiteConnection = SQLite.SQLiteConnection;
using TableAttribute = SQLite.TableAttribute;

namespace LocalDB_test;

internal class Program
{
    public enum Operation
    {
        Append=0,
        Delete=1,
        Replace=2,
        QueryAll=3,
        QueryById=4,
        Quit=5,
    }

    private  static IDBContext _context;

    static void Main(string[] args)
    {
        var localDBPath = @"C:\Users\33008\AppData\Local\ShadowBot\book.db";
        _context= new DBContext(localDBPath);
        InitMenu();
        MainLoop();
    }

    private static void MainLoop()
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (Int32.TryParse(input, out int operationCode))
            {
                var operation = (Operation)operationCode;
                switch (operation)
                {
                    case Operation.Append:
                        HandleAppend();
                        break;
                    case Operation.Delete:
                        HandleDelete();
                        break;
                    case Operation.Replace:
                        HandleReplace();
                        break;
                    case Operation.QueryAll:
                        ListAllBooks();
                        break;
                    case Operation.QueryById:
                        HandleQueryById();
                        break;
                    case Operation.Quit:
                        return;
                    default:
                        Console.WriteLine("*****Invalid operation,please try again*****");
                        break;
                }
            }
            else
            {
                Console.WriteLine("*****Invalid operation,please try again*****");
            }
            InitMenu();
        }
    }

    private static void ListAllBooks()
    {

    }

    private static void InitMenu()
    {
        Console.WriteLine("=====================Welcome to my library========================");
        Console.WriteLine("Avaliable operations:");
        Console.WriteLine("Append a new book:0");
        Console.WriteLine("Delete an exits book:1");
        Console.WriteLine("Replace a old book:2");
        Console.WriteLine("List all books:3");
        Console.WriteLine("Query a book by id:4");
        Console.WriteLine("Quit:5");
    }

    private static void HandleAppend()
    {
        Console.WriteLine("Inter your bookName:");
        var input = Console.ReadLine();
        while (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Invalid book name,try again");
            Console.WriteLine("Inter your bookName:");
            input= Console.ReadLine();
        }

        var book = new Book(input);
        _context.AppendBook(book);
    }

    private static void HandleDelete()
    {
        Console.WriteLine("input the name of the book you want to delete:");
        var input = Console.ReadLine();
        while (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("input the name of the book you want to delete:");
            input = Console.ReadLine();
        }

        //_context.DeleteBook();
    }

    private static void HandleReplace()
    {

    }

    private static void HandleQueryById()
    {

    }

    private static void HandleQuit()
    {

    }
 
}


public interface IDBContext
{
    Task<bool> AppendBook(Book book);
    Task<bool> DeleteBook(string name);
    Task<bool> ReplaceBook(Book book);
    Task<IEnumerable<Book>> GetBooks();
    Task<Book> GetBook(int id);
}

public class DBContext:IDBContext
{
    private readonly SQLiteConnection _context;

    public  DBContext(string dbPath)
    {
        _context = new SQLiteConnection(dbPath);
        _context.CreateTable<Book>();
    } 

    public Task<bool> AppendBook(Book book)
    {
        try
        {
            _context.Insert(book);
            Console.WriteLine("Book appened successfully");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteBook(string name)
    {
        try
        {
            var books = _context.Query<Book>("Select * from books");
            var book=books.FirstOrDefault(x=>x.Name==name);
            _context.Delete(book);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ReplaceBook(Book book)
    {
        try
        {
            var books = _context.Query<Book>("Select * from books");
            var oldBook = books.FirstOrDefault(x => x.Id == book.Id);
            if (oldBook == null)
            {
                Console.WriteLine("the book not exist");
                return Task.FromResult(false);
            }

            _context.Delete(oldBook);
            _context.Insert(book);
            Console.WriteLine("Book replaced successfully");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<Book>> GetBooks()
    {
        var books = _context.Query<Book>("Select * from books");
        return Task.FromResult(books.AsEnumerable());
    }

    public Task<Book> GetBook(int id)
    {
        var books = _context.Query<Book>("Select * from books");
        var book= books.FirstOrDefault(x=>x.Id==id);
        return Task.FromResult(book);
    }
}


[Table("books")]
public class Book
{
    public string Name { get; set; }
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    public Book(string name)
    {
        Name=name;
    }

    public Book()
    {
    }
}