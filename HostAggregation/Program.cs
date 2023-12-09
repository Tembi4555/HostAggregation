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
                        //string[] data = File.ReadAllLines(f);

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

            //var readF = readFiles.Where(s=> s.ShortName == "file00000 — копия.txt");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            //IEnumerable<HostRangeFull> res = RangeAllocationService.Helpers.Parser.GetListHostRangeFullFromReadFileWithParallel(readFiles);
            IEnumerable<HostRangesFull> hostsFromFileList = RangeAllocationService.Helpers.Parser.GetListHostRangeFullFromReadFile(readFiles);

            Console.WriteLine($"Работа по переводу считанных фалов в HostRangeFull выполнена за {sw.ElapsedMilliseconds}");

            //var aggregationData = HostRanking.GetRankingHost(hostsFromFileList).OrderBy(s => s.Ranges[0]).ToList();

            var aggregationData = HostRanking.GetRankingHost(hostsFromFileList).OrderBy(s => s.Ranges[0])
                .Where(h => h.ExInClusionFlag == ExInClusionFlag.Include).ToList();

            string stringForSave = RangeAllocationService.Helpers.Parser.StringFromHostRangeShort(aggregationData);

            string pathForSave = FileManagementService.FileManagemer.GetPathForSave();

            string messagePath = FileManagementService.FileManagemer.SaveFile(pathForSave, stringForSave);

            Console.WriteLine($"Программа выполнена.\nРезультирующий файл отчета можете просмотреть в " 
                + messagePath );

            Console.WriteLine($"Время работы программы {sw.ElapsedMilliseconds}");
            sw.Stop();

            Console.ReadKey();
        }
    }
}
