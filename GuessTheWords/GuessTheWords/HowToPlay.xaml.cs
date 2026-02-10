using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client_GuessTheWords

{
    /// <summary>
    /// Interaction logic for HowToPlay.xaml
    /// </summary>
    public partial class HowToPlay : Window
    {
        public HowToPlay()
        {
            InitializeComponent();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            return;
        }
    }
}
