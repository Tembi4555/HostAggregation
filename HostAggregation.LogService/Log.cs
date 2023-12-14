using System.Text;

namespace HostAggregation.LogService
{
    public class Log
    {
        private static StringBuilder _errorJournal = new StringBuilder("Имя файла | Номер строки | Текст ошибки");
        private static StringBuilder _infoJournal = new StringBuilder();
        private static int _countAddedInfoString = 0;
        private static bool fixLog = true;

        public static void AddError(string error)
        {
            if(fixLog)
                _errorJournal.Append($"\r\n{error}");
        }

        public static void AddError(StringBuilder error)
        {
            if (fixLog)
                _errorJournal.Append($"\r\n{error}");
        }

        public static void AddInfo(string info)
        {
            if (fixLog)
            {
                _infoJournal.Append($"\r\n{info}");
                _countAddedInfoString++;
                if (_countAddedInfoString == 1000000)
                {
                    Parallel.Invoke(() => CreateInfoJournal());
                }
            }
            
        }

        public static string CreateErrorJournal()
        {
            if(fixLog)
            {
                string path = FileManagementService.FileManagemer.GetPathForSave("LogJournals", "Error.txt");
                string result = FileManagementService.FileManagemer.SaveFile(path, _errorJournal.ToString());
                _errorJournal.Clear();
                return result;
            }
            return "Лог не записывался";
        }

        public static string CreateInfoJournal()
        {
            if (fixLog)
            {
                string path = FileManagementService.FileManagemer.GetPathForSave("LogJournals",
                $"Info-{DateTime.Now.Millisecond}.txt");
                string result = FileManagementService.FileManagemer.SaveFile(path, _infoJournal.ToString());
                _infoJournal.Clear();
                _countAddedInfoString = 0;
                return result;
            }
            return "Лог не записывался";
        }
    }
}
