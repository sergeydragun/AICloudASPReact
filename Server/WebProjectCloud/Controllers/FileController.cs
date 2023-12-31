﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebProjectCloud.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using System.Runtime.CompilerServices;
using System;
using static System.Net.WebRequestMethods;

namespace WebProjectCloud.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _enviroment;
        private readonly ApplicationContext _context;

        public FileController(ApplicationContext context,IWebHostEnvironment enviroment)
        {
            _enviroment = enviroment;
            _context = context;
        }

        [Route("{id}")]
        [HttpGet]
        public JsonResult Get(int id)
        {
            var files = _context.Files.Where(x => x.FolderModel.Id == id).ToList();
            return new JsonResult(files);
        }

        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Post(int id)
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files.FirstOrDefault();

                if (postedFile == null)
                    return StatusCode(404);

                string? filename = postedFile?.FileName;
                var physicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos", filename);
                 
                physicalPath = WorkWithFiles.GetFileName(physicalPath);

                var resultName = Path.GetFileName(physicalPath);

                using(var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await postedFile.CopyToAsync(stream);
                }

                var folder = await _context.FolderModel.FirstOrDefaultAsync(x => x.Id == id);

                FileModel file = new FileModel() {Name = resultName, Path = physicalPath, FolderModel = folder };
                _context.Files.Add(file);
                _context.SaveChanges();
                return new JsonResult("picture added");
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }

        [Route("GetFile/{id}/{filename}")]
        [HttpGet]
        public async Task<IActionResult> GetFile(int id, string filename)
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos", filename);
            string file_type;
            var forfile_type = new FileExtensionContentTypeProvider().TryGetContentType(file_path, out file_type);
            
            return PhysicalFile(file_path, file_type); 
        }

        [Route("{id}/{filename}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id, string filename)
        {
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos", filename);
            if(System.IO.File.Exists(filepath))
                System.IO.File.Delete(filepath);

            var file = await _context.Files.Where(f => f.FolderModel.Id == id && f.Name == filename).FirstOrDefaultAsync();
            _context.Files.Remove(file);
            _context.SaveChanges();

            return StatusCode(204);
        }


    }

    public static class WorkWithFiles
    {
        public static string GetFileName(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {               
                return GetFileName(Path.ChangeExtension(filePath, null) + " (c)" + Path.GetExtension(filePath));
            }
            return filePath;
        }
    }
    
}