using CefSharp;
using MeBrowser.Helpers;
using MeBrowser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MeBrowser.ViewModel
{
    public class DialogBox : BindableBase
    {
        //demo datacontext cho dialogview
        public static DialogBox Default { get; } = new DialogBox();
        //SetDialogType phải được gọi trước khi muốn hiển thị bất kì dialog nào
        public static ICommand SetDialogType { get; } = new RelayCommand(obj => DialogType = (Type)obj);
        //type cuả window hiện dialog
        public static Type? DialogType { get; private set; }
        private static readonly List<DispatcherTimer> timers = new List<DispatcherTimer>();
        private static readonly List<int> counters = new List<int>();
        //mỗi dialog được tạo ra sẽ được gắn 1 instance của DialogBox làm datacontext
        public object Message { get => GetProperty<object>(); set => SetProperty(value); }
        public string Title { get => GetProperty<string>(); set => SetProperty(value); }
        public Visibility OkVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility CancelVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility YesVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility NoVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility ErrorVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility InfoVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility WarningVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public Visibility QuestionVisibility { get => GetProperty<Visibility>(); set => SetProperty(value); }
        public DialogBoxResult Result { get; private set; }
        public ICommand? SendResult { get; private set; }
        public ICommand? CloseDialog { get; private set; }
        public ICommand? DragMove { get; private set; } 
        private DialogBox()
        { 

        }
        public static DialogBoxResult Show(object message, string title = "Thông báo", DialogBoxButton button = DialogBoxButton.OK, DialogBoxIcon icon = DialogBoxIcon.Error) => new DialogBox().ShowDialog(message, title, button, icon);
        private DialogBoxResult ShowDialog(object message, string title = "Thông báo", DialogBoxButton button = DialogBoxButton.OK, DialogBoxIcon icon = DialogBoxIcon.Error)
        {
            if(DialogType == null) throw new NullReferenceException($"SetDialogType phải được gọi với parameter là 1 Type của Window hoặc các lớp con của nó.");

            if (Activator.CreateInstance(DialogType) is not Window dialog) throw new Exception($"SetDialogType phải được gọi với parameter là 1 Type của Window hoặc các lớp con của nó.");

            if (Application.Current.Dispatcher.CheckAccess())
            {
                dialog.Owner = Application.Current.MainWindow;
            }
            
            SendResult = new RelayCommand(obj =>
            {
                if(obj is DialogBoxResult result) Result = result;
                dialog.Close();
            });
            CloseDialog = new RelayCommand(obj => dialog.Close());
            DragMove = new RelayCommand(obj => dialog.DragMove());
            dialog.DataContext = this;

            if(message is string stringMsg)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = stringMsg,
                    TextWrapping = TextWrapping.Wrap
                };
                Message = textBlock;
                textBlock.SetBinding(FrameworkElement.MaxWidthProperty, new System.Windows.Data.Binding
                {
                    RelativeSource = new System.Windows.Data.RelativeSource { Mode = System.Windows.Data.RelativeSourceMode.FindAncestor, AncestorType = typeof(Label) },
                    Path = new PropertyPath(Label.ActualWidthProperty),
                });
            }
            else
            {
                Message = message;
            }

            Title = title;

            if (button == DialogBoxButton.OK)
            {
                OkVisibility = Visibility.Visible;
                CancelVisibility = Visibility.Collapsed;
                YesVisibility = Visibility.Collapsed;
                NoVisibility = Visibility.Collapsed;
            }
            if (button == DialogBoxButton.OKCancel)
            {
                OkVisibility = Visibility.Visible;
                CancelVisibility = Visibility.Visible;
                YesVisibility = Visibility.Collapsed;
                NoVisibility = Visibility.Collapsed;
            }
            if (button == DialogBoxButton.YesNo)
            {
                OkVisibility = Visibility.Collapsed;
                CancelVisibility = Visibility.Collapsed;
                YesVisibility = Visibility.Visible;
                NoVisibility = Visibility.Visible;
            }
            if (button == DialogBoxButton.YesNoCancel)
            {
                OkVisibility = Visibility.Collapsed;
                CancelVisibility = Visibility.Visible;
                YesVisibility = Visibility.Visible;
                NoVisibility = Visibility.Visible;
            }

            if (icon == DialogBoxIcon.Error)
            {
                ErrorVisibility = Visibility.Visible;
                InfoVisibility = Visibility.Collapsed;
                WarningVisibility = Visibility.Collapsed;
                QuestionVisibility = Visibility.Collapsed;
            }
            if (icon == DialogBoxIcon.Information)
            {
                ErrorVisibility = Visibility.Collapsed;
                InfoVisibility = Visibility.Visible;
                WarningVisibility = Visibility.Collapsed;
                QuestionVisibility = Visibility.Collapsed;
            }
            if (icon == DialogBoxIcon.Warning)
            {
                ErrorVisibility = Visibility.Collapsed;
                InfoVisibility = Visibility.Collapsed;
                WarningVisibility = Visibility.Visible;
                QuestionVisibility = Visibility.Collapsed;
            }
            if (icon == DialogBoxIcon.Question)
            {
                ErrorVisibility = Visibility.Collapsed;
                InfoVisibility = Visibility.Collapsed;
                WarningVisibility = Visibility.Collapsed;
                QuestionVisibility = Visibility.Visible;
            }

            Result = DialogBoxResult.None;

            dialog.ShowDialog();

            return Result;
        }

        public static void ShowError(object message, string title = "ERROR", int countStart = 10) => new DialogBox().ShowErrorCountDown(message, title, countStart);
 
        private void ShowErrorCountDown(object message, string title, int countStart)
        {
            if (countStart == 0) return;

            if (DialogType == null) throw new NullReferenceException($"SetDialogType phải được gọi với parameter là 1 Type của Window hoặc các lớp con của nó.");

            if (Activator.CreateInstance(DialogType) is not Window dialog) throw new Exception($"SetDialogType phải được gọi với parameter là 1 Type của Window hoặc các lớp con của nó.");

            if (Application.Current.Dispatcher.CheckAccess())
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            SendResult = new RelayCommand(obj =>
            {
                Result = (DialogBoxResult)obj;
                dialog.Close();
            });
            CloseDialog = new RelayCommand(obj => dialog.Close());
            DragMove = new RelayCommand(obj => dialog.DragMove());
            dialog.DataContext = this;

            if (message is string stringMsg)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = stringMsg,
                    TextWrapping = TextWrapping.Wrap
                };
                Message = textBlock;
                textBlock.SetBinding(TextBlock.MaxWidthProperty, new System.Windows.Data.Binding
                {
                    RelativeSource = new System.Windows.Data.RelativeSource { Mode = System.Windows.Data.RelativeSourceMode.FindAncestor, AncestorType = typeof(Label) },
                    Path = new PropertyPath(Label.ActualWidthProperty),
                });
            }
            else
            {
                Message = message;
            }

            Title = title;

            OkVisibility = Visibility.Visible;
            CancelVisibility = Visibility.Collapsed;
            YesVisibility = Visibility.Collapsed;
            NoVisibility = Visibility.Collapsed;

            ErrorVisibility = Visibility.Visible;
            InfoVisibility = Visibility.Collapsed;
            WarningVisibility = Visibility.Collapsed;
            QuestionVisibility = Visibility.Collapsed;

            var timer = new DispatcherTimer();
            timers.Add(timer);
            var count = countStart;
            counters.Add(count);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                dialog.Title = title + $" ({count})";
                AttachedManager.SetStringValue(dialog, $" ({count})");

                if(count == 0)
                {
                    timer.Stop();
                    dialog.Close();
                    timers.Remove(timer);
                    counters.Remove(count);
                }
                else
                {
                    --count;
                }
            };
            dialog.Closing += (s, e) =>
            {
                timer.Stop();
                timers.Remove(timer);
                counters.Remove(count); 
            };
            timer.Start();
            dialog.Show();
        }
    }
}
