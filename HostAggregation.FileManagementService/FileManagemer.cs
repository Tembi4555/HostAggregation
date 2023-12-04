using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace HostAggregation.FileManagementService
{
    /// <summary>
    /// Работа с файлами и директориями
    /// </summary>
    public class FileManagemer
    {
        /// <summary>
        /// Чтение всех файлов в указанном каталоге и его подкаталогах.
        /// </summary>
        /// <param name="root">Имя каталога</param>
        /// <param name="action">Действия с результатами</param>
        public static void TraverseTreeParallelForEach(string root, Action<string> action)
        {
            // Переменная для подсчета количества файлов и задания таймера выполнения.
            int fileCount = 0;
            var sw = Stopwatch.StartNew();

            // Переменная для определения следует ли распараллеливать обработку файлов в каждой папке на основе количества процессоров .
            int procCount = Environment.ProcessorCount;

            // Структура данных для хранения имен подпапок, которые необходимо проверить на наличие файлов.
            Stack<string> dirs = new Stack<string>();

            if (!Directory.Exists(root))
            {
                throw new ArgumentException(
                    "Указанный каталог не существует.", nameof(root));
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = { };
                string[] files = { };

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                // Исключение, если пользователь не имеет прав на текущую директорию.
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                // Вызывается, если другой процесс удалил каталог после того, как мы получили его имя.
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Выполнять параллельно, если в каталоге достаточно файлов.
                // В противном случае выполните последовательно. Файлы открываются и обрабатываются
                try
                {
                    if (files.Length < procCount)
                    {
                        foreach (var file in files)
                        {
                            action(file);
                            fileCount++;
                        }
                    }
                    else
                    {
                        Parallel.ForEach(files, () => 0,
                            (file, loopState, localCount) =>
                            {
                                action(file);
                                return (int)++localCount;
                            },
                            (c) =>
                            {
                                Interlocked.Add(ref fileCount, c);
                            });
                    }
                }
                catch (AggregateException ae)
                {
                    ae.Handle((ex) =>
                    {
                        if (ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine(ex.Message);
                            return true;
                        }

                        return false;
                    });
                }

                // Поместите подкаталоги в стек для обхода.
                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            // For diagnostic purposes.
            Console.WriteLine("Processed {0} files in {1} milliseconds", fileCount, sw.ElapsedMilliseconds);
        }

        /*public static async Task TraverseTreeParallelForEachAsync(string root, Task<Action<string>> action)
        {
            // Переменная для подсчета количества файлов и задания таймера выполнения.
            int fileCount = 0;
            var sw = Stopwatch.StartNew();

            // Переменная для определения следует ли распараллеливать обработку файлов в каждой папке на основе количества процессоров .
            int procCount = Environment.ProcessorCount;

            // Структура данных для хранения имен подпапок, которые необходимо проверить на наличие файлов.
            Stack<string> dirs = new Stack<string>();

            if (!Directory.Exists(root))
            {
                throw new ArgumentException(
                    "Указанный каталог не существует.", nameof(root));
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = { };
                string[] files = { };

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                // Исключение, если пользователь не имеет прав на текущую директорию.
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                // Вызывается, если другой процесс удалил каталог после того, как мы получили его имя.
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                // Выполнять параллельно, если в каталоге достаточно файлов.
                // В противном случае выполните последовательно. Файлы открываются и обрабатываются
                try
                {
                    if (files.Length < procCount)
                    {
                        foreach (var file in files)
                        {
                            action(file);
                            fileCount++;
                        }
                    }
                    else
                    {
                        *//*await Parallel.ForEachAsync(files, () => 0,
                            (file, loopState, localCount) =>
                            {
                                action(file);
                                return (int)++localCount;
                            },
                            (c) =>
                            {
                                Interlocked.Add(ref fileCount, c);
                            });*//*
                        await Parallel.ForEachAsync(files, async(file, loopState) =>
                        {
                            var a = await action(file);
                            return (int)++localCount;
                        });
                    }
                }
                catch (AggregateException ae)
                {
                    ae.Handle((ex) =>
                    {
                        if (ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine(ex.Message);
                            return true;
                        }

                        return false;
                    });
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            // For diagnostic purposes.
            Console.WriteLine("Processed {0} files in {1} milliseconds", fileCount, sw.ElapsedMilliseconds);
        }*/

        public static string SaveFile(string path, string textForSave)
        {
            try
            {
                using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    // преобразуем строку в байты
                    byte[] buffer = Encoding.Default.GetBytes(textForSave);
                    // запись массива байтов в файл
                    fstream.Write(buffer, 0, buffer.Length);
                    return $"Файл сохранен в {path}";
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> SaveFileAsync(string path, string textForSave)
        {
            try
            {
                using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    // преобразуем строку в байты
                    byte[] buffer = Encoding.Default.GetBytes(textForSave);
                    // запись массива байтов в файл
                    await fstream.WriteAsync(buffer, 0, buffer.Length);
                    return $"Файл сохранен в {path}";
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
