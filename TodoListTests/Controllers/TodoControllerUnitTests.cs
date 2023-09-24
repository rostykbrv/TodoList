using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Models;
using TodoList.Controllers;
using TodoList.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace TodoListTests.Controllers
{
    public class TodoControllerUnitTests
    {
        private readonly TodoDbContext _context;
        private readonly TodoController _controller;

        public TodoControllerUnitTests()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                              .UseInMemoryDatabase(databaseName: "TodoTestDb")
                              .Options;

            _context = new TodoDbContext(options);
            _controller = new TodoController(_context);
        }

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {
            // Arrange
            var items = new List<TodoItem>
            {
                new TodoItem { Title = "Item 1", Description = "Item 1 description" },
                new TodoItem { Title = "Item 2", Description = "Item 2 description" },
            };
            _context.AddRange(items);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTodoItems();

            // Assert
            Assert.Equal(2, result.Value.Count());

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetById_ItemExists_ReturnsItem()
        {
            // Arrange
            var item = new TodoItem { Title = "Item", Description = "Item description" };
            _context.Add(item);
            await _context.SaveChangesAsync();
            var id = item.Id;

            // Act
            var result = await _controller.GetTodoItem(id);

            // Assert
            Assert.Equal(id, result.Value.Id);

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetById_ItemNotFound_ReturnsNotFound()
        {
            // Arrange
            var id = 100;

            // Act
            var result = await _controller.GetTodoItem(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task CreateTodoItem_AddsNewItem()
        {
            // Arrange
            var newItem = new TodoItem { Title = "New Item", Description = "New item description" };

            // Act
            var result = await _controller.CreateTodoItem(newItem);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdItem = Assert.IsType<TodoItem>(createdAtActionResult.Value);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await _context.TodoItems.CountAsync());
            Assert.Equal(newItem.Title, createdItem.Title);
            Assert.Equal(newItem.Description, createdItem.Description);

            _context.Database.EnsureDeleted(); 
        }

        [Fact]
        public async Task UpdateTodoItem_ItemExists_UpdatesItem()
        {
            var item = new TodoItem { Title = "Item", Description = "Item description" };
            _context.Add(item);
            await _context.SaveChangesAsync();
            var id = item.Id;

            // Detach the item from the context
            _context.Entry(item).State = EntityState.Detached;

            var updatedItem = new TodoItem { Id = id, Title = "Updated Item", Description = "Updated item description" };

            // Act
            var result = await _controller.UpdateTodoItem(id, updatedItem);
            await _context.SaveChangesAsync();

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedItemFromDb = await _context.TodoItems.FindAsync(id);
            Assert.Equal(updatedItem.Title, updatedItemFromDb.Title);
            Assert.Equal(updatedItem.Description, updatedItemFromDb.Description);

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task UpdateTodoItem_WrongId_ReturnsNotFoundResult()
        {
            // Arrange
            var updatedItem = new TodoItem { Title = "Updated Item", Description = "Updated item description" };

            // Act
            var result = await _controller.UpdateTodoItem(100, updatedItem);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task DeleteTodoItem_DeleteItem_ReturnsNoContent()
        {
            // Arrange
            var item = new TodoItem { Title = "Item", Description = "Item description" };
            _context.Add(item);
            await _context.SaveChangesAsync();
            var id = item.Id;

            // Act
            var result = await _controller.DeleteTodoItem(id);
            await _context.SaveChangesAsync();

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.TodoItems.FindAsync(id));

            _context.Database.EnsureDeleted();
        }
    }
}
