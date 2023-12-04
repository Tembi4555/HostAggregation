using FileManagementService.Models;
using HostAggregation.FileManagementService;
using HostAggregation.HelpersService.Helpers;
using HostAggregation.RangeAllocationService;
using HostAggregation.RangeAllocationService.Helpers;
using HostAggregation.RangeAllocationService.Models;
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
                        //string data = File.ReadAllText(f);

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

            var readFilesOrderBy = readFiles.OrderBy(r => r.ShortName);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            ReadFile readFile = readFilesOrderBy.FirstOrDefault();

            List<ReadFile> list = new List<ReadFile>(); list.Add(readFile);

            IEnumerable<HostRangeFull> res = RangeAllocationService.Helpers.Parser.GetListHostRangeFullFromReadFile(list);

            Console.WriteLine($"Работа по переводу считанных фалов в HostRangeFull выполнена за {sw.ElapsedMilliseconds}");

            sw.Stop();
            Console.WriteLine($"Результат {res.Count()} хостов");

            List<HostRangeShort> shorts = HostRanking.GetRankingHost(res);

            string result = RangeAllocationService.Helpers.Parser.StringFromHostRangeShort(shorts);
            var GetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\hosts.txt";
            string saveRes = FileManagementService.FileManagemer.SaveFile(path, result);
            Console.WriteLine(saveRes);

            Console.ReadKey();
        }
    }
}
