using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileUtility
{
    public partial class Form1 : Form
    {
        private string origem;
        private string destino;

        public Form1()
        {
            InitializeComponent();
        }

        public string MyProperty { get; set; }

        private static void ReplaceTextInFiles(DirectoryInfo directory, string term, string replace)
        {
            int tentativas = 0;
            inicio:
            ReplaceInFiles(directory, term, replace);
            var dirs = directory.GetDirectories("*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                try
                {
                    if (dir.Name.Contains(term))
                        Directory.Move(dir.FullName, dir.Parent.FullName + "/" + dir.Name.Replace(term, replace));
                }
                catch
                {
                    if (tentativas < 3)
                    {
                        tentativas++;
                        goto inicio;
                    }
                }
            }
        }

        static bool IsBinary(string content)
        {
            var bytes = Encoding.ASCII.GetBytes(content);
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] > 127)
                    return true;
            return false;
        }

        public static void ReplaceInFiles(DirectoryInfo dir, string term, string replace)
        {
            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                if (
                    file.Extension != ".dll" 
                    && file.Extension != ".dylib" 
                    && file.Extension != ".so" 
                    && file.Extension != ".pdb" 
                    && file.Extension != ".cache")
                {
                    try
                    {

                        string strFile = File.ReadAllText(file.FullName);

                        if (IsBinary(strFile))
                            continue;

                        strFile = Regex.Replace(strFile, $@"{term}", replace);
                        File.WriteAllText(file.FullName, strFile);

                        if (file.Name.Contains(term))
                            Directory.Move(file.FullName, file.Directory + "/" + file.Name.Replace(term, replace));
                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                }
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (origem != null && destino != null && CopiarDiretorios(origem, destino))
            {
                var dir = new DirectoryInfo(destino);
                ReplaceTextInFiles(dir, textBox1.Text, textBox2.Text);
            }
        }

        private bool CopiarDiretorios(string origem, string destino)
        {
            if (Directory.Exists(destino))
            {
                if (MessageBox.Show("O diretorio de destino ja existe, continuar?", "!!!!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }

            foreach (string dir in Directory.GetDirectories(origem, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(destino, dir.Substring(origem.Length + 1)));
            }

            foreach (string file_name in Directory.GetFiles(origem, "*", SearchOption.AllDirectories))
            {
                File.Copy(file_name, Path.Combine(destino, file_name.Substring(origem.Length + 1)));
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnOrig_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                var di = new DirectoryInfo(dlg.SelectedPath);
                this.origem = dlg.SelectedPath;
                this.textBox1.Text = di.Name;
                btnOrig.Text = this.origem;
            }
        }

        private void btnDest_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                var fi = new FileInfo(dlg.FileName);
                this.destino = fi.FullName;
                textBox2.Text = fi.Name;
                btnDest.Text = this.destino;
            }
        }
    }
}
