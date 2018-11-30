using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UserGroupVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> groupList = new List<string>();
        private ObservableCollection<string> matchList = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        public delegate void Add(TreeViewItem treeviewitem);

        private void GroupSearch(ADGroup branch, Principal group, PrincipalContext context)
        {

            GroupPrincipal grp = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, group.SamAccountName);
            PrincipalSearchResult<Principal> grouplist = grp.GetGroups();
            if (grouplist.Count<Principal>() > 0)
            {
                ADGroup maingroup = new ADGroup
                {
                    Groupname = group.Name
                };
                foreach (var groupmember in grouplist)
                {
                    GroupSearch(maingroup, groupmember, context);
                }
                groupList.Add(group.Name);
                branch.Subgroups.Add(maingroup);
            }
            else
            {
                groupList.Add(group.Name);
                branch.Subgroups.Add(new ADGroup() { Groupname = group.Name });
            }
        }
        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            treeviewGroups.Visibility = Visibility.Hidden;
            treeviewGroups.Items.Clear();            
            Helper items = new Helper
            {
                UserDomain = textboxDomain.Text,
                Username = textboxUsername.Text
            };

            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(items);
        }

        private void textboxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxDomain.Text != "" && textboxUsername.Text != "")
            {
                buttonSearch.IsEnabled = true;
            }
        }
        private void textboxDomain_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxDomain.Text != "" && textboxUsername.Text != "")
            {
                buttonSearch.IsEnabled = true;
            }
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Helper helper = (Helper)e.Argument;
            try
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, helper.UserDomain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, helper.Username);
                PrincipalSearchResult<Principal> grouplist = user.GetGroups();
                ADGroup root = new ADGroup
                {
                    Groupname = user.Name,
                    Subgroups = new ObservableCollection<ADGroup>()
                };
                int count = 1;
                int maximum = grouplist.Count<Principal>();

                foreach (Principal group in grouplist)
                {
                    GroupSearch(root, group, ctx);
                    int progress = Convert.ToInt32(((decimal)count / (decimal)maximum) * 100);
                    (sender as BackgroundWorker).ReportProgress(progress);
                    count++;
                }
                e.Result = root;
            }
            catch (Exception exc)
            {
                switch(exc.GetType().ToString())
                {
                    case "System.NullReferenceException":
                        MessageBox.Show("User Not Found", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case "System.DirectoryServices.AccountManagement.PrincipalServerDownException":
                        MessageBox.Show("Server Down", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        break;
                    default:
                        MessageBox.Show(exc.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }             
            }
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 100)
            {
                progressBar.Visibility = Visibility.Hidden;
                txtblockProgress.Visibility = Visibility.Hidden;
            }
            else
            {
                progressBar.Visibility = Visibility.Visible;
                txtblockProgress.Visibility = Visibility.Visible;
                progressBar.Value = e.ProgressPercentage;
                txtblockProgress.Text = e.ProgressPercentage.ToString();
            }            
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ADGroup wholetree = (ADGroup)e.Result;
                TreeViewItem rootNode = new TreeViewItem
                {
                    Header = wholetree.Groupname
                };
                BuildTree(wholetree.Subgroups, rootNode);
                treeviewGroups.Visibility = Visibility.Visible;
                treeviewGroups.Items.Add(rootNode);

                if (treeviewGroups.Items.Count > 0)
                {
                    labelGroupSearch.IsEnabled = true;
                    textboxGroupSearch.IsEnabled = true;
                }
            }
        }
        void BuildTree(ObservableCollection<ADGroup> treenodes, TreeViewItem parent)
        {
            foreach (ADGroup node in treenodes)
            {
                TreeViewItem treenode = new TreeViewItem
                {
                    Header = node.Groupname
                };
                if (node.Subgroups.Count<ADGroup>() > 0)
                {
                    parent.Items.Add(treenode);
                    parent.Items.SortDescriptions.Clear();
                    parent.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
                    BuildTree(node.Subgroups, treenode);
                }
                else
                {
                    parent.Items.Add(treenode);
                    parent.Items.SortDescriptions.Clear();
                    parent.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
                }
            }
        }

        private void TextboxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            matchList.Clear();
            Regex newmatch = new Regex(textboxGroupSearch.Text, RegexOptions.IgnoreCase);
            foreach(string group in groupList)
            {
                Match match = newmatch.Match(group);
                if(match.Success)
                {
                    matchList.Add(group);
                }
            }
            datagridResults.ItemsSource = matchList;
        }
    }
}
