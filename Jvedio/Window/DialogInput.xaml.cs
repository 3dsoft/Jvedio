﻿using System;
using System.Windows;
using System.Windows.Input;

namespace Jvedio
{
    /// <summary>
    /// DialogInput.xaml 的交互逻辑
    /// </summary>
    public partial class DialogInput : Window
    {

        public DialogInput(Window window,string title, string defaultContent = "")
        {
            InitializeComponent();

            TitleTextBlock.Text = title;
            ContentTextBox.Text = defaultContent;
            this.Owner = window;
            this.Left = window.Left+15;
            this.Top = window.Top+15;
            this.Height = window.Height-30;
            this.Width = window.Width-30;

        }

        public string Text
        {
            get { return ContentTextBox.Text; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ContentTextBox.SelectAll();
            ContentTextBox.Focus();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ContentTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.DialogResult = true;
            else if (e.Key == Key.Escape)
                this.DialogResult = false;
            else if (e.Key == Key.Delete)
                ContentTextBox.Text = "";


        }
    }
}
