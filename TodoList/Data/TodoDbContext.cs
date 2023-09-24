using Microsoft.EntityFrameworkCore;
using TodoList.Models;

namespace TodoList.Data
{
    public class TodoDbContext: DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext>options):base(options)
        {
           
        }

        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
