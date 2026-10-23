using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskPlanner.Data;
using TaskPlanner.Models;
using TaskPlanner.Services;
using WinForms = System.Windows.Forms;
using WpfApplication = System.Windows.Application;
using WpfMessageBox = System.Windows.MessageBox;

namespace TaskPlanner
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<TaskItem> _allTasks = new();
        private readonly ObservableCollection<TaskItem> _visibleTasks = new();
        private readonly ObservableCollection<TaskItem> _overdueTasks = new();

        private DateTime _selectedDate = DateTime.Today;
        private bool _isChangingDate;
        private bool _isInitializing = true;
        private WinForms.NotifyIcon? _notifyIcon;
        private bool _isExplicitExit = false;

        public MainWindow()
        {
            InitializeComponent();

            foreach (var task in TaskRepository.Load())
            {
                _allTasks.Add(task);
            }

            TasksList.ItemsSource = _visibleTasks;
            OverdueTasksList.ItemsSource = _overdueTasks;

            DateSelector.SelectedDate = _selectedDate;

            InitTrayIcon();

            try
            {
                StartupCheckBox.IsChecked = StartupService.IsEnabled();
            }
            catch
            {
                StartupCheckBox.IsChecked = false;
            }
            finally
            {
                _isInitializing = false;
            }

            RefreshLists();
        }

        private void InitTrayIcon()
        {
            System.Drawing.Icon? appIcon = null;

            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icono.ico");

                if (System.IO.File.Exists(iconPath))
                {
                    appIcon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    // Intentar extraer el icono asignado al propio ejecutable
                    appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? string.Empty);
                }
            }
            catch
            {
                appIcon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon = new WinForms.NotifyIcon
            {
                Icon = appIcon ?? System.Drawing.SystemIcons.Application,
                Visible = true, 
                Text = "TaskPlanner"
            };

            _notifyIcon.DoubleClick += (s, e) => RestoreWindow();

            var contextMenu = new WinForms.ContextMenuStrip();
            contextMenu.Items.Add("Abrir TaskPlanner", null, (s, e) => RestoreWindow());
            contextMenu.Items.Add(new WinForms.ToolStripSeparator());
            contextMenu.Items.Add("Salir completamente", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExplicitExit)
            {
                e.Cancel = true;
                this.Hide();

                _notifyIcon?.ShowBalloonTip(
                    2000,
                    "TaskPlanner",
                    "La aplicación sigue ejecutándose en segundo plano.",
                    WinForms.ToolTipIcon.Info);
            }
        }

        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ExitApplication()
        {
            _isExplicitExit = true;
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            WpfApplication.Current.Shutdown();
        }

        private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            try
            {
                if (StartupCheckBox.IsChecked == true)
                {
                    StartupService.Enable();
                }
                else
                {
                    StartupService.Disable();
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    $"No se pudo cambiar la configuración de inicio automático.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Yesterday_Click(object sender, RoutedEventArgs e)
        {
            SelectDate(_selectedDate.AddDays(-1));
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            SelectDate(DateTime.Today);
        }

        private void Tomorrow_Click(object sender, RoutedEventArgs e)
        {
            SelectDate(_selectedDate.AddDays(1));
        }

        private void DateSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isChangingDate || DateSelector.SelectedDate == null)
                return;

            _selectedDate = DateSelector.SelectedDate.Value.Date;
            RefreshLists();
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date.Date;

            _isChangingDate = true;
            DateSelector.SelectedDate = _selectedDate;
            _isChangingDate = false;

            RefreshLists();
        }

        private void RefreshLists()
        {
            _visibleTasks.Clear();
            _overdueTasks.Clear();

            foreach (var task in _allTasks
                .Where(t => t.Date.Date == _selectedDate)
                .OrderBy(t => t.IsCompleted))
            {
                _visibleTasks.Add(task);
            }

            foreach (var task in _allTasks
                .Where(t => t.Date.Date < DateTime.Today
                         && t.Date.Date != _selectedDate
                         && !t.IsCompleted)
                .OrderBy(t => t.Date)
                .ThenBy(t => t.Title))
            {
                _overdueTasks.Add(task);
            }

            SelectedDateTitle.Text =
                _selectedDate == DateTime.Today
                    ? "Tareas de hoy"
                    : _selectedDate == DateTime.Today.AddDays(1)
                        ? "Tareas de mañana"
                        : _selectedDate == DateTime.Today.AddDays(-1)
                            ? "Tareas de ayer"
                            : $"Tareas del {_selectedDate:dd/MM/yyyy}";

            ProgressText.Text =
                $"{_visibleTasks.Count(t => t.IsCompleted)} de " +
                $"{_visibleTasks.Count} tareas completadas";

            EmptyStateText.Visibility =
                _visibleTasks.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            OverduePanel.Visibility =
                _overdueTasks.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (AddTomorrowTaskButton != null)
            {
                AddTomorrowTaskButton.Visibility =
                    _selectedDate == DateTime.Today
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
        }

        private void AddTaskForSelectedDate_Click(object sender, RoutedEventArgs e)
        {
            OpenAddTaskModal(_selectedDate);
        }

        private void AddTomorrowTask_Click(object sender, RoutedEventArgs e)
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            OpenAddTaskModal(tomorrow);
        }

        private void OpenAddTaskModal(DateTime targetDate)
        {
            var existingTasks = new ObservableCollection<TaskItem>(
                _allTasks.Where(t => t.Date.Date == targetDate));

            string dayLabel = targetDate == DateTime.Today
                ? "hoy"
                : targetDate == DateTime.Today.AddDays(1)
                    ? "mañana"
                    : targetDate.ToString("dd/MM/yyyy");

            var dialog = new Window
            {
                Title = $"Planificar tareas para {dayLabel}",
                Width = 460,
                Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#17191F")),
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1F2F5"))
            };

            var rootGrid = new Grid
            {
                Margin = new Thickness(20)
            };
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 1. Formulario
            var inputPanel = new StackPanel();
            Grid.SetRow(inputPanel, 0);

            inputPanel.Children.Add(new TextBlock
            {
                Text = $"Nueva tarea para {dayLabel} ({targetDate:dd/MM/yyyy})",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A7E3C0")),
                Margin = new Thickness(0, 0, 0, 10)
            });

            var inputGrid = new Grid();
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var input = new System.Windows.Controls.TextBox
            {
                FontSize = 14,
                Padding = new Thickness(8),
                Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#22252D")),
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1F2F5")),
                BorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3A3E49")),
                CaretBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1F2F5"))
            };
            Grid.SetColumn(input, 0);

            var addButton = new System.Windows.Controls.Button
            {
                Content = "Agregar",
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(8, 0, 0, 0),
                Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A7E3C0")),
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#17251D")),
                FontWeight = FontWeights.SemiBold,
                IsDefault = true
            };
            Grid.SetColumn(addButton, 1);

            inputGrid.Children.Add(input);
            inputGrid.Children.Add(addButton);
            inputPanel.Children.Add(inputGrid);
            rootGrid.Children.Add(inputPanel);

            // 2. Encabezado de lista previa
            var listHeader = new TextBlock
            {
                Text = $"Tareas ya agendadas para {dayLabel}:",
                FontSize = 13,
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999EAD")),
                Margin = new Thickness(0, 16, 0, 8)
            };
            Grid.SetRow(listHeader, 1);
            rootGrid.Children.Add(listHeader);

            // 3. Lista previa
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(scrollViewer, 2);

            var tasksItemsControl = new ItemsControl
            {
                ItemsSource = existingTasks
            };

            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2E38")));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(10, 8, 10, 8));
            borderFactory.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 6));

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Title"));
            textBlockFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1F2F5")));
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, 13.0);

            borderFactory.AppendChild(textBlockFactory);
            tasksItemsControl.ItemTemplate = new DataTemplate { VisualTree = borderFactory };

            scrollViewer.Content = tasksItemsControl;
            rootGrid.Children.Add(scrollViewer);

            // 4. Botón Listo
            var closeButton = new System.Windows.Controls.Button
            {
                Content = "Listo",
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 14, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#292C35")),
                Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1F2F5")),
                IsCancel = true
            };
            Grid.SetRow(closeButton, 3);
            rootGrid.Children.Add(closeButton);

            addButton.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(input.Text))
                {
                    WpfMessageBox.Show(
                        dialog,
                        "Escribí el nombre de la tarea.",
                        "TaskPlanner",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                string titleClean = input.Text.Trim();

                if (existingTasks.Any(t => t.Title.Equals(titleClean, StringComparison.OrdinalIgnoreCase)))
                {
                    WpfMessageBox.Show(
                        dialog,
                        "Ya existe una tarea con ese nombre para este día.",
                        "TaskPlanner",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var newTask = new TaskItem
                {
                    Title = titleClean,
                    Date = targetDate,
                    IsCompleted = false
                };

                _allTasks.Add(newTask);
                existingTasks.Add(newTask);
                TaskRepository.Save(_allTasks);

                input.Clear();
                input.Focus();
            };

            dialog.Content = rootGrid;
            dialog.Loaded += (_, _) => input.Focus();

            dialog.ShowDialog();

            RefreshLists();
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button &&
                button.DataContext is TaskItem task)
            {
                _allTasks.Remove(task);
                TaskRepository.Save(_allTasks);
                RefreshLists();
            }
        }

        private void TaskCompleted_Changed(object sender, RoutedEventArgs e)
        {
            TaskRepository.Save(_allTasks);
            RefreshLists();
        }
    }
}