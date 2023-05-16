using LibHIRT.Common;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Xceed.Wpf.AvalonDock.Controls;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for BytesReadView.xaml
    /// </summary>
    public partial class BytesReadView : UserControl
    {
        private BinaryReader stream;

        public string Byte
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadSByte().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string UByte
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadByte().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string Int16
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadInt16().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {

                    return "";
                }
            }
        }
        public string UInt16
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadUInt16().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {

                    return "";
                }
            }
        }

        public string Int32
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadInt32().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {

                    return "";
                }
            }
        }
        public string Mmr3
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = Mmr3HashLTU.getMmr3HashFromInt(stream.ReadInt32());
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {

                    return "";
                }
            }
        }
        public string UInt32
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadUInt32().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {

                    return "";
                }
            }
        }

        public string Int64
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadInt64().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }
        public string UInt64
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadUInt64().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string Float16
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadHalf().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }
        public string Float32
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadSingle().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }
        public string Float64
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadDouble().ToString();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string String4
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp =new string(stream.ReadChars(4));
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string String4Rev
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    var chars = stream.ReadChars(4);
                    Array.Reverse(chars);
                    string temp = new string(chars);
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }

        public string StringNZ
        {
            get
            {
                try
                {
                    if (stream == null)
                        return "";
                    var pos = stream.BaseStream.Position;
                    string temp = stream.ReadStringNullTerminated();
                    stream.BaseStream.Position = pos;
                    return temp;
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }


        public static readonly DependencyProperty FileStreamProperty = DependencyProperty.Register("FileStream", typeof(Stream), typeof(BytesReadView), new
           PropertyMetadata(null, new PropertyChangedCallback(OnFileStreamChanged)));

        public Stream FileStream
        {
            get { return (Stream)GetValue(FileStreamProperty); }
            set { SetValue(FileStreamProperty, value); }
        }

        private static void OnFileStreamChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            BytesReadView UserControl1Control = d as BytesReadView;
            UserControl1Control.OnFileStreamChanged(e);
        }

        private void OnFileStreamChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                stream= new BinaryReader((Stream)e.NewValue);
            }// RefreshJsonTree(e.NewValue.ToString());
        }
        public BytesReadView()
        {
            InitializeComponent();
            DataContext= this;  
        }

        public void Refresh() {
            BindingOperations.GetBindingExpressionBase((TextBox)TbUbyte, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbByte, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbUInt16, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbInt16, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbUInt32, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbInt32, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbMmr3, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbUInt64, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbInt64, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbFloat16, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbFloat32, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbFloat64, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbString4, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbString4Rev, TextBox.TextProperty).UpdateTarget();
            BindingOperations.GetBindingExpressionBase((TextBox)TbStringNZ, TextBox.TextProperty).UpdateTarget();
        }


    }
}
