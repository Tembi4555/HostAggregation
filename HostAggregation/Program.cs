using FileManagementService.Models;
using HostAggregation.FileManagementService;
using HostAggregation.HelpersService.Helpers;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace HostAggregation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string directoryName = @"D:\projects\example-generator\Output";
            Console.WriteLine("Введите директорию для работы с файлами");
            //string directoryName = Console.ReadLine();
            directoryName = directoryName?.Replace('"', ' ')?.Trim();
            List<ReadFile> readFiles = new List<ReadFile>();
            try
            {
                FileManagemer.TraverseTreeParallelForEach(directoryName, (f) =>
                {
                    // Exceptions are no-ops.
                    try
                    {
                        FileInfo file = new FileInfo(f);
                        byte[] data = File.ReadAllBytes(f);

                        ReadFile readFile = new ReadFile()
                        {
                            FullName = file.FullName,
                            ShortName = file.Name,
                            CreationTime = file.CreationTime,
                            Size = file.Length,
                            DataFromFile = data
                        };
                        readFiles.Add(readFile);
                    }
                    catch (FileNotFoundException) { }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                    catch (SecurityException) { }

                    Console.WriteLine(f);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            List<string> strings = new();

            var sw = Stopwatch.StartNew();

            foreach (var r in readFiles)
            {
                string arr = Encoding.UTF8.GetString(r.DataFromFile);
                strings.Add(arr);
            }
            Console.WriteLine($"Enocoding выполнялся {sw.ElapsedMilliseconds}");
            strings.Clear();

            sw = Stopwatch.StartNew();

            foreach (var r in readFiles)
            {
                int count = r.DataFromFile.Length;
                var stringBuilder = new StringBuilder(count * 2);

                for (var i = 0; i < count; ++i)
                    stringBuilder.Append(r.DataFromFile[i]);

                var res = stringBuilder.ToString();
                strings.Add(res);
            }

            


            Console.WriteLine($"Stringuilder выполнялся {sw.ElapsedMilliseconds}");
            Console.ReadKey();
        }
    }
}
