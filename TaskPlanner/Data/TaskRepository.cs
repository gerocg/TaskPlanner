
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskPlanner.Models;

namespace TaskPlanner.Data
{
    public static class TaskRepository
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "TaskPlanner");

        private static readonly string FilePath =
            Path.Combine(FolderPath, "tasks.json");

        public static List<TaskItem> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new List<TaskItem>();

                string json = File.ReadAllText(FilePath);

                return JsonSerializer.Deserialize<List<TaskItem>>(json)
                       ?? new List<TaskItem>();
            }
            catch (JsonException)
            {
                System.Windows.MessageBox.Show(
                    "No se pudieron interpretar las tareas guardadas.",
                    "TaskPlanner",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return new List<TaskItem>();
            }
            catch (IOException)
            {
                System.Windows.MessageBox.Show(
                    "No se pudo leer el archivo de tareas.",
                    "TaskPlanner",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return new List<TaskItem>();
            }
        }

        public static void Save(IEnumerable<TaskItem> tasks)
        {
            Directory.CreateDirectory(FolderPath);

            string json = JsonSerializer.Serialize(
                tasks,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(FilePath, json);
        }
    }
}
