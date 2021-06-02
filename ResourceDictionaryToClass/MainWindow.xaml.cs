using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;

namespace ResourceDictionaryToClass
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string classTemplate = @"
using System;
using System.Windows;

namespace LauncherX.Class.Helper
{
    public static class LangHelper
    {
        public static String GetStr(string key)
        {
            return Application.Current.Resources.MergedDictionaries[3][key]?.ToString() ?? $""{key.ToUpper()}"";
        }
        {0}
    }
}
";

        private readonly string propertyTemplate = @"
        /// <summary>
        /// {1}
        /// </summary>
        public static string {0} => GetStr(nameof({0}));
        ";

        public MainWindow()
        {
            InitializeComponent();
            resourceText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xaml");
            classText.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var inputFolderStr = inputFolder.Text;
            var outputFolderStr = outputFolder.Text;
            if (!Directory.Exists(inputFolderStr))
            {
                MessageBox.Show(inputFolderStr + "不存在");
                return;
            }

            if (!Directory.Exists(outputFolderStr))
            {
                MessageBox.Show(outputFolderStr + "不存在");
                return;
            }

            var enumerateFiles = Directory.EnumerateFiles(inputFolderStr);
            foreach (var file in enumerateFiles)
                if (Path.GetExtension(file) == ".xaml")
                {
                    var fs = new FileStream(file, FileMode.Open);
                    var reader = new StreamReader(fs);
                    var readToEnd = reader.ReadToEnd();
                    var classStr = Transform(readToEnd);
                    reader.Close();
                    fs.Close();
                    fs = new FileStream(Path.Combine(outputFolderStr, Path.GetFileNameWithoutExtension(file) + ".cs"),
                        FileMode.Create);
                    var writer = new StreamWriter(fs);
                    writer.Write(classStr);
                    writer.Close();
                    fs.Close();
                }

            MessageBox.Show("成功");
        }

        private string Transform(string input)
        {
            try
            {
                var strReader = new StringReader(input);
                var xmlReader = new XmlTextReader(strReader);
                var builder = new StringBuilder();
                if (XamlReader.Load(xmlReader) is ResourceDictionary dic)
                    foreach (var dicKey in dic.Keys)
                    {
                        var value = dic[dicKey] as string;
                        builder.AppendFormat(propertyTemplate, dicKey, value);
                    }

                var result = classTemplate.Replace("{0}", builder.ToString());
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            classText.Text = Transform(resourceText.Text);
        }
    }
}