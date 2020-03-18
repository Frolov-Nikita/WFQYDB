using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xaml;

namespace TuiExperCF
{
    class Program
    {

        static ViewModels.MyVm context = new ViewModels.MyVm();


        private static void WindowsHost_KeyUp(object sender, ConsoleFramework.Events.KeyEventArgs args)
        {
            context.MyProperty += args.UnicodeChar;
        }

        public static void Main(string[] args)
        {
            Window window = (Window)ConsoleApplication.LoadFromXaml("TuiExperCF.Views.Win2.xml", context);
            window.KeyUp += WindowsHost_KeyUp;
            WindowsHost wh = new WindowsHost { Name = "temp" };
            wh.Show(window);
            ConsoleApplication.Instance.Run(wh);
            return;

            using (ConsoleApplication application = ConsoleApplication.Instance)
            {
                Panel panel = new Panel();
                panel.Name = "panel1";
                panel.HorizontalAlignment = HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.XChildren.Add(new TextBlock()
                {
                    Name = "label1",
                    Text = "Label1",
                    Margin = new Thickness(1, 2, 1, 0)
                    //,Visibility = Visibility.Collapsed
                });
                panel.XChildren.Add(new TextBlock()
                {
                    Name = "label2",
                    Text = "Label2_____",
                    HorizontalAlignment = HorizontalAlignment.Right
                });
                TextBox textBox = new TextBox()
                {
                    MaxWidth = 10,
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Size = 15,
                    Text = "{Binding Path=MyProperty}",
                };
                Button button = new Button()
                {
                    Name = "button1",
                    Caption = "Button!",
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                button.OnClick += (sender, eventArgs) => {
                    Debug.WriteLine("Click");
                    MessageBox.Show("Окно сообщения", "Внимание ! Тестовое сообщение", delegate (MessageBoxResult result) { });
                    Control label = panel.FindDirectChildByName("label1");
                    if (label.Visibility == Visibility.Visible)
                    {
                        label.Visibility = Visibility.Collapsed;
                    }
                    else if (label.Visibility == Visibility.Collapsed)
                    {
                        label.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        label.Visibility = Visibility.Visible;
                    }
                    label.Invalidate();
                };
                ComboBox comboBox = new ComboBox()
                {
                    //                        Width = 14
                    //HorizontalAlignment = HorizontalAlignment.Stretch
                };
                comboBox.Items.Add("Сделать одно");
                comboBox.Items.Add("Сделать второе");
                comboBox.Items.Add("Ничего не делать");
                ListBox listbox = new ListBox();
                listbox.Items.Add("First item");
                listbox.Items.Add("second item1!!!!!!1fff");
                listbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                //listbox.Width = 10;
                panel.XChildren.Add(comboBox);
                panel.XChildren.Add(button);
                panel.XChildren.Add(textBox);
                panel.XChildren.Add(listbox);

                //application.Run(panel);
                WindowsHost windowsHost = new WindowsHost()
                {
                    Name = "WindowsHost"
                };
                Window window1 = new Window
                {
                    X = 5,
                    Y = 4,
                    //MinHeight = 100,
                    //MaxWidth = 30,
                    //Width = 10,
                    Height = 20,
                    Name = "Window1",
                    Title = "Window1",
                    Content = panel
                };
                
                GroupBox groupBox = new GroupBox();
                groupBox.Title = "Группа";
                ScrollViewer scrollViewer = new ScrollViewer();
                ListBox listBox = new ListBox();
                for (int i = 0; i < 30; i++)
                {
                    listBox.Items.Add(string.Format("Длинный элемент {0}", i));
                }
                //                listBox.Items.Add( "Длинный элемент" );
                //                listBox.Items.Add("Длинный элемент 2");
                //                listBox.Items.Add("Длинный элемент 3");
                //                listBox.Items.Add("Длинный элемент 4");
                //                listBox.Items.Add("Длинный элемент 5");
                //                listBox.Items.Add("Длинный элемент 6");
                //                listBox.Items.Add("Длинный элемент 700");
                listBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                listBox.VerticalAlignment = VerticalAlignment.Stretch;
                scrollViewer.Content = listBox;
                //                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
                scrollViewer.HorizontalScrollEnabled = true;

                groupBox.Content = scrollViewer;

                ComboBox combo = new ComboBox();
                combo.ShownItemsCount = 10;
                for (int i = 0; i < 30; i++)
                {
                    combo.Items.Add(string.Format("Длинный элемент {0}", i));
                }
                //                groupBox.Content = combo;

                groupBox.HorizontalAlignment = HorizontalAlignment.Stretch;


                windowsHost.Show(new Window()
                {
                    X = 30,
                    Y = 6,
                    //MinHeight = 10,
                    //MinWidth = 10,
                    Height = 14,
                    Name = "LongTitleWindow",
                    Title = "Очень длинное название окна",
                    Content = groupBox,
                    DataContext = context,
                }) ;
                windowsHost.Show(window1);
                //windowsHost.Show(createdFromXaml);
                //textBox.SetFocus(); todo : научиться задавать фокусный элемент до добавления в визуальное дерево
                //application.TerminalSizeChanged += ( sender, eventArgs ) => {
                //    application.CanvasSize = new Size(eventArgs.Width, eventArgs.Height);
                //   application.RootElementRect = new Rect(new Size(eventArgs.Width, eventArgs.Height));
                // };
                //windowsHost.Width = 80;
                //windowsHost.Height = 20;
                application.Run(windowsHost);//, new Size(80, 30), Rect.Empty);

                windowsHost.KeyUp += WindowsHost_KeyUp;
            }
        }

    }
}
