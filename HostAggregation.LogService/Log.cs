using System.Text;

namespace HostAggregation.LogService
{
    public class Log
    {
        private static StringBuilder _errorJournal = new StringBuilder("Имя файла | Номер строки | Текст ошибки");
        private static StringBuilder _infoJournal = new StringBuilder();
        private static int _countAddedInfoString = 0;

        public static void AddError(string error)
        {
            _errorJournal.Append($"\r\n{error}");
        }

        public static void AddError(StringBuilder error)
        {
            _errorJournal.Append($"\r\n{error}");
        }

        public static void AddInfo(string info)
        {
            _infoJournal.Append( $"\r\n{info}");
            _countAddedInfoString++;
            if(_countAddedInfoString == 1000000)
            {
                Parallel.Invoke(() => CreateInfoJournal());
                
                _infoJournal.Clear();
                _countAddedInfoString = 0;
            }
        }

        public static string CreateErrorJournal()
        {
            string path = FileManagementService.FileManagemer.GetPathForSave("LogJournals", "Error.txt");
            string result = FileManagementService.FileManagemer.SaveFile(path, _errorJournal.ToString());
            return result;
        }

        public static string CreateInfoJournal()
        {
            string path = FileManagementService.FileManagemer.GetPathForSave("LogJournals", 
                $"Info-{DateTime.Now.Millisecond}.txt");
            string result = FileManagementService.FileManagemer.SaveFile(path, _infoJournal.ToString());
            return result;
        }
    }
}
