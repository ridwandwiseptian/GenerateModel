using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using GenerateModel.Models;
using Microsoft.AspNetCore.Mvc;

namespace GenerateModel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(GenerateModelInput model)
        {
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "json_file", "Dataset.json");
                string jsonContent = System.IO.File.ReadAllText(filePath);


                var mapping = JsonSerializer.Deserialize<DatabaseTypeMapping>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (mapping != null)
                {
                    var databaseNames = mapping.GetType()
                               .GetProperties()
                               .Select(p => p.Name)
                               .ToList();

                    if (databaseNames.Count() != 0)
                    {
                        model.ListDatabase = databaseNames;
                    }
                }

                if (model.Input != null && model.DatabaseSelected != null)
                {
                    var mappingSql = JsonSerializer.Deserialize<DatabaseTypeMapping>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    var db = model.DatabaseSelected;

                    List<DatabaseType> TypeSQL = new List<DatabaseType>();
                    var property = mappingSql?.GetType().GetProperty(db);
                    if (property != null)
                    {
                        var databaseMapping = property.GetValue(mappingSql);
                        if (databaseMapping is Dictionary<string, string> mappingDictionary)
                        {
                            foreach (var kvp in mappingDictionary)
                            {
                                TypeSQL.Add(new DatabaseType
                                {
                                    Key = kvp.Key,
                                    Value = kvp.Value
                                });
                            }
                        }
                    }


                    string output = "";
                    string outputClass = "";
                    List<string> lines = model.Input
                        .Replace(",", "") // Menghapus koma untuk parsing lebih akurat
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    foreach (var line in lines)
                    {
                        var baris = line.Trim();

                        string[] parts = baris.Replace("[", "").Replace("]", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length < 3)
                        {
                            continue;
                        }

                        string columnName = parts[0]; // Nama Kolom
                        string dataType = parts[1];   // Tipe Data
                        string length = "";           // Panjang (jika ada)
                        string nullable = "";         // NULL atau NOT NULL

                        if (dataType.Contains("("))
                        {
                            int start = dataType.IndexOf('(');
                            length = dataType.Substring(start + 1, dataType.Length - start - 2); // Ambil isi dalam kurung
                            dataType = dataType.Substring(0, start); // Ambil tipe data tanpa panjang
                        }

                        if (parts.Length >= 4)
                        {
                            nullable = parts[^2] == "NOT" && parts[^1] == "NULL" ? "NOT NULL" : "NULL";
                        }
                        else
                        {
                            nullable = parts[^1];
                        }

                        output += $"Column Name: {columnName}<br />";
                        output += $"  - Data Type: {dataType}<br />";
                        if (!string.IsNullOrEmpty(length))
                            output += $"  - Length: {length}<br />";
                        output += $"  - Nullable: {nullable}<br /><br />";



                        var get_ = TypeSQL.Where(b => b.Key == dataType.ToLower().Trim()).FirstOrDefault();

                        //[Column(TypeName = "nvarchar(250)")]
                        //[MaxLength(250)]
                        //[Display(Name = "Nama Mahasiswa")]
                        //public string? NAMA { get; set; }
                        string res_tipe_data = "";

                        if (nullable == "NOT NULL")
                        {
                            res_tipe_data = get_.Value.ToString();

                            outputClass += "public " + res_tipe_data + " " + columnName + " { get; set; }" + "<br />";
                        }
                        else
                        {
                            res_tipe_data = get_.Value.ToString() + "?";

                            outputClass += "public " + res_tipe_data + " " + columnName + " { get; set; }" + "<br />";
                        }



                    }

                    Console.WriteLine(output); // Cetak hasil untuk debugging


                    model.OutputColumn = output;
                    model.OutputClassModel = outputClass;
                }
            }
            catch (Exception)
            {

                model.Pesan = "Error";
            }
          
           

            return View(model);
        }

        public IActionResult Privacy()
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "json_file", "Dataset.json");
            string jsonContent = System.IO.File.ReadAllText(filePath);


            var mapping = JsonSerializer.Deserialize<DatabaseTypeMapping>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(mapping);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
