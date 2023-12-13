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
            Console.WriteLine("Введите директорию для работы с файлами");
            string directoryName = Console.ReadLine();
            if(String.IsNullOrEmpty(directoryName))
                directoryName = @"C:\example-generator\Output";
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

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            IEnumerable<HostRangesFull> hostsFromFileList = RangeAllocationService.Helpers.Parser.GetListHostRangeFullFromReadFile(readFiles);

            Console.WriteLine($"Работа по переводу считанных файлов в HostRangeFull выполнена за {sw.ElapsedMilliseconds}");

            List<HostRangesBase> aggregationData = HostRanking.GetRankingHost(hostsFromFileList);
                

            string stringForSave = RangeAllocationService.Helpers.Parser.StringFromHostRangeShort(aggregationData);

            string pathForSave = FileManagemer.GetPathForSave();

            string messagePath = FileManagemer.SaveFile(pathForSave, stringForSave);

            Console.WriteLine($"Программа выполнена.\nРезультирующий файл отчета можете просмотреть в " 
                + messagePath );

            Console.WriteLine($"Время работы программы {sw.ElapsedMilliseconds}");
            sw.Stop();

            Console.ReadKey();
        }
    }
}
