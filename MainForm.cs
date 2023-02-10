using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhotoEditorSimple
{
    public partial class MainForm : Form
    {
        private string imagePath;
        private string firstPath;
        private string selectedDir;
        private string parentDir;
        private int counter = 0;
        public string photoRootDirectory;
        private string selDirectory;
        private List<FileInfo> photoFiles;
        private CancellationTokenSource cancellationTokenSource;
        private bool isClicked = false;

        public MainForm()
        {
            InitializeComponent();

         
            photoRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            selDirectory = photoRootDirectory;

            treeView1.Nodes.Add(PopulateTreeView(photoRootDirectory));
            LoadListView();
            treeView1.ExpandAll();
           
        }

        private TreeNode PopulateTreeView(string path)
        {
            var directory = new DirectoryInfo(path).Name;

            firstPath = path.Split(Path.DirectorySeparatorChar).Last();
            parentDir = path.Substring(0, path.LastIndexOf('\\'));

            TreeNode treeResult = new TreeNode(firstPath);
            foreach(var subDirectory in Directory.GetDirectories(path))
            {
                treeResult.Nodes.Add(PopulateTreeView(subDirectory));
            }
            return treeResult;
        }

        private async Task LoadListView()
        {
           cancellationTokenSource = new CancellationTokenSource();
            progressBar1.Visible = true;


            selectedDir = parentDir + "\\" + selDirectory;

            if (counter == 0)
            {
                selectedDir = selDirectory;
            }
            if(selDirectory == photoRootDirectory.Split(Path.DirectorySeparatorChar).Last())
            {
                selectedDir = photoRootDirectory;
            }
          

            DirectoryInfo dir = new DirectoryInfo(selectedDir);
            photoFiles = new List<FileInfo>();
            photoView.LargeImageList = new ImageList();
            photoView.SmallImageList = new ImageList();

            photoView.LargeImageList.ImageSize = new Size(256, 256);
            photoView.SmallImageList.ImageSize = new Size(64, 64);

            await Task.Run(() =>
            {
                foreach (FileInfo file in dir.GetFiles("*.jpg"))
                {
                    try
                    {
                        Image img = LoadImage(file.FullName);
                        photoFiles.Add(file);
                        Invoke((Action)delegate ()
                        {
                            photoView.SmallImageList.Images.Add(img);
                            photoView.LargeImageList.Images.Add(img);
                        });

                    }
                    catch { }

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                }

                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Invoke((Action)delegate ()
                    {
                        if (detailToolStripMenuItem.Checked == true)
                        {
                            DetailListView();
                        }
                        else if (smallToolStripMenuItem.Checked == true)
                        {
                            SmallListView();
                        }
                        else
                        {
                            LargeListView();
                        }
                    });
                }
            });

            progressBar1.Visible = false;
            cancellationTokenSource = null;
            counter++;
            

        }

        private void DetailListView()
        {
            photoView.Items.Clear();
            photoView.Columns.Clear();

            photoView.View = View.Details;
            photoView.Columns.Add("Name", 300, HorizontalAlignment.Left);
            photoView.Columns.Add("Date", 100, HorizontalAlignment.Left);
            photoView.Columns.Add("File Size", 100, HorizontalAlignment.Left);
            photoView.FullRowSelect = true;

            int pIndex = 0;
            foreach (FileInfo file in photoFiles)
            {
                ListViewItem photoItem = new ListViewItem(file.Name, pIndex);
                photoItem.SubItems.Add(file.LastWriteTime.ToString());

                long numBytes = file.Length;
                if (numBytes / 1000000 != 0)
                {
                    numBytes = numBytes / 1000000;
                    photoItem.SubItems.Add(numBytes.ToString() + " MB");
                }
                else
                {
                    numBytes = numBytes / 1000;
                    photoItem.SubItems.Add(numBytes.ToString() + " kB");
                }
                photoItem.Tag = file.Directory;
                photoView.Items.Add(photoItem);
                pIndex++;
            }
        }

        private void SmallListView()
        {
            photoView.Items.Clear();
            photoView.View = View.SmallIcon;
            int pIndex = 0;
            foreach (FileInfo file in photoFiles)
            {
                ListViewItem photoItem = new ListViewItem(file.Name, pIndex);
                photoItem.Tag = file.Directory;
                photoView.Items.Add(photoItem);
                pIndex++;
            }
        }

        private void LargeListView()
        {
            photoView.Items.Clear();
            photoView.View = View.LargeIcon;
            int pIndex = 0;
            foreach (FileInfo file in photoFiles)
            {
                ListViewItem photoItem = new ListViewItem(file.Name, pIndex);
                photoItem.Tag = file.Directory;
                photoView.Items.Add(photoItem);
                pIndex++;
            }
        }
        private Image LoadImage(string filename)
        {
                // Use this method to load images so the image files do not remain locked
                byte[] bytes = File.ReadAllBytes(filename);
                MemoryStream ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
        }
        private void photoMainListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            //locate on disk
            isClicked = true;

        }

        private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Cancel the task (assume the token is not null when task is running)
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                // Wait until the task has been cancelled
                while (cancellationTokenSource != null)
                {
                    Application.DoEvents();
                }
            }

            selDirectory = treeView1.SelectedNode.Text;
            await LoadListView();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm abtform = new AboutForm();
            abtform.ShowDialog(this);
        }

        private void detailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detailToolStripMenuItem.Checked = true;
            smallToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = false;
            DetailListView();
        }

        private void smallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detailToolStripMenuItem.Checked = false;
            smallToolStripMenuItem.Checked = true;
            largeToolStripMenuItem.Checked = false;
            SmallListView();
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detailToolStripMenuItem.Checked = false;
            smallToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = true;
            LargeListView();
        }

        private void photoView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DirectoryInfo homeDir = new DirectoryInfo(selectedDir);

            if (photoView.SelectedItems.Count == 0)
            {
                return;
            }
            imagePath = homeDir + @"\" + photoView.SelectedItems[0].Text;

            EditForm editForm = new EditForm(imagePath);
            Console.WriteLine(imagePath);
            editForm.ShowDialog();
        }

        private void locateOnDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo homeDir = new DirectoryInfo(selectedDir);

            if (photoView.SelectedItems.Count == 0)
            {
                if (isClicked == false)
                {
                    MessageBox.Show("First select an image, then choose this option to locate on disk.");
                }
                return;
            }
            imagePath = homeDir + @"\" + photoView.SelectedItems[0].Text;

            if (isClicked == true)
            {
                ProcessStartInfo process = new ProcessStartInfo();
                process.Arguments = imagePath;
                process.WorkingDirectory = photoRootDirectory;
                process.UseShellExecute = false;
                process.FileName = imagePath;

                imagePath = System.IO.Path.GetFullPath(imagePath);
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{imagePath}\"");

            } 
        }

        private void selectRootFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://www.c-sharpcorner.com/uploadfile/mahesh/folderbrowserdialog-in-C-Sharp/
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                photoRootDirectory = folderBrowserDialog.SelectedPath;
                Environment.SpecialFolder root = folderBrowserDialog.RootFolder;
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(PopulateTreeView(photoRootDirectory));
            }

        }



    }
}
