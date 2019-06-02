using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EguiReact.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;

namespace EguiReact.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public JsonResult List()
        {
            List<BookModel> _books = new List<BookModel>();
            this.GetData(_books);
            return Json(_books);
        }

        [HttpPost]
        public IActionResult Update([FromBody]BookModel _book)
        {
            List<BookModel> _books = new List<BookModel>();
            GetData(_books);
            var elem = _books.FirstOrDefault(b => b.Id == _book.Id);
            if (elem == null)
            {
                return Error();
            }
            else
            {
                elem.Author = _book.Author;
                elem.Title = _book.Title;
                elem.Year = _book.Year;
            }
            SetData(_books);
            return Ok(Json(elem));
        }

        [HttpPost]
        public IActionResult Create([FromBody]BookModel _book)
        {
            List<BookModel> _books = new List<BookModel>();
            GetData(_books);
            _book.Id = 0;
            foreach (BookModel b in _books)
            {
                if (b.Id > _book.Id)
                {
                    _book.Id = b.Id;
                }
            }
            _book.Id++;
            _books.Add(_book);
            SetData(_books);
            return Ok(Json(_book));
        }

        [HttpPost]
        public IActionResult Delete([FromBody]List<int> _ids)
        {
            List<BookModel> _books = new List<BookModel>();
            GetData(_books);
            _books = _books.Where(b => !_ids.Contains(b.Id)).ToList();
            SetData(_books);
            return Ok(Json(_books));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void GetData (List<BookModel> _books)
        {
            try
            {
                StreamReader file = new StreamReader("LibraryData.txt");
                String line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] itemData = line.Split(';');
                    if (itemData.Length != 4)
                        throw new IOException("File is corrupted (record length mismatch)!");
                    if ((Regex.Match(itemData[3], @"[^\d]")).Success)
                        throw new IOException("File is corrupted (Year must contain number)!");
                    if ((Regex.Match(itemData[0], @"[^\d]")).Success)
                        throw new IOException("File is corrupted (Id must contain number)!");
                    BookModel newItem = new BookModel
                    {
                        Id = int.Parse(itemData[0]),
                        Author = itemData[1],
                        Title = itemData[2],
                        Year = itemData[3]
                    };
                    _books.Add(newItem);
                }
                file.Close();
            }
            catch (IOException e)
            {
                Error();
            }
        }

        private void SetData(List<BookModel> _books)
        {
            using (StreamWriter file = new StreamWriter("LibraryData.txt"))
            {
                foreach (BookModel item in _books)
                {
                    file.WriteLine(item.Id + ";" + item.Author + ";" + item.Title + ";" + item.Year);
                }
            }
        }
    }
}
