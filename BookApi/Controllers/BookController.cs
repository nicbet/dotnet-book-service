using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase {

        private readonly BookContext _context;

        public BookController(BookContext context) {
            _context = context;
            
            // If no Books exist, let's create one or two, just to test!
            if (_context.Books.Count() == 0 ) {
                _context.Books.Add(new Book { Name = "A Brief History of Time", Author = "Stephen Hawking"});
                _context.Books.Add(new Book { Name = "The Universe in a Nutshell", Author = "Stephen Hawking"});
                _context.SaveChanges();
            }
        } 

        [HttpGet]
        public IEnumerable<Book> Get() {
            return _context.Books.ToList();
        }
    }
}