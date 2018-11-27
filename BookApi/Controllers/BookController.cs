using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BookApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase {

        // Application configuration
        private IConfiguration _config;

        // Database Context
        private readonly BookContext _context;

        // Redis Cache
        private readonly IDistributedCache _cache;
        private readonly int _cache_ttl;

        // Constructor with DI
        public BookController(BookContext context, IDistributedCache cache, IConfiguration config) {
            _context = context;
            _cache = cache;
            _config = config;
            _cache_ttl = int.Parse(_config["REDIS_TTL"] ?? "60");

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

        [HttpGet("{id}")]
        public ActionResult<Book> GetById(long id)
        {
            // Attempt to retrieve from cache first
            var item = RetrieveFromCache(id);
            return item;
        }

        private void SaveToCache(Book book)
        {
            var json = JsonConvert.SerializeObject(book);
            _cache.SetString(book.Id.ToString(), json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cache_ttl)
            });
            Console.Out.WriteLine("Saved " + book.Id + " to cache!");
        }

        private Book RetrieveFromCache(long id)
        {
            var key = id.ToString();
            var json = _cache.GetString(key);
            if (json == null)
            {
                Console.Out.WriteLine("Cache miss for " + id + ", fetching from database...");
                var item =_context.Books.Find(id);
                SaveToCache(item);
                return item;
            }
            else
            {
                Console.Out.WriteLine("Cache hit for " + id);
                return JsonConvert.DeserializeObject<Book>(json);

            }
        }
    }
}
