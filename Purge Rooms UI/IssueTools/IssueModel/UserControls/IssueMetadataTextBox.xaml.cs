using System.Windows;
using System.Windows.Controls;

namespace Purge_Rooms_UI.IssueTools.IssueModel.UserControls
{
    public partial class IssueMetadataTextBox : UserControl
    {
        public IssueMetadataTextBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        private string _labelText;
        public string LabelText
        {
            get { return _labelText; }
            set { _labelText = value;}
        }

        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set { _inputText = value; }
        }

        //public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(
        //    "InputText", typeof(string), typeof(IssueMetadataTextBox), new PropertyMetadata(default(string)));
        //public string InputText
        //{
        //    get { return (string)GetValue(InputTextProperty); }
        //    set { SetValue(InputTextProperty, value); }
        //}
    }
}
