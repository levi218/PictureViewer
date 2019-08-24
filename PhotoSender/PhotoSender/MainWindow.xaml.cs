using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        User[] users;
        string filePath;

        public MainWindow(string filePath) : this()
        {
            this.filePath = filePath;
            txtPath.Text = filePath;
        }

        public MainWindow()
        {
            InitializeComponent();

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            p.StartInfo.FileName = "node";
            p.StartInfo.Arguments = @"fb_api\index.js -f";
            p.Start();
            
            string output = p.StandardOutput.ReadToEnd();
            
            p.WaitForExit();

            Match match = Regex.Match(output, @"(?:\[)([\s\S]*)(?:\])");
            Console.WriteLine(match.Length);
            string jsonData = match.Value;

            JavaScriptSerializer js = new JavaScriptSerializer();
            users = js.Deserialize<User[]>(jsonData);

            lvFriendList.ItemsSource = users;

            //    MyObject foo = (MyObject)MyListView.SelectedItems[0];
            lvFriendList.SelectionMode = SelectionMode.Single;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            User u = (User)lvFriendList.SelectedItem;
            string filePath = txtPath.Text;
            string testUser = "100002995566344";
            if (u != null)
            {
                // Start the child process.
                Process p = new Process();
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                p.StartInfo.FileName = "node";
                p.StartInfo.Arguments = @"fb_api\index.js -p "+"\""+ u.UserId + "\" \""+filePath+"\"";
                p.Start();

                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();
            }
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string content = txtFilter.Text;
            foreach(User u in users)
            {
                if (u.Name.StartsWith(content))
                {
                    lvFriendList.SelectedItem = u;
                    lvFriendList.ScrollIntoView(u);
                    break;
                }
            }
                
        }
    }
}
