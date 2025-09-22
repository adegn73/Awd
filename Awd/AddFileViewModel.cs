using Awd.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Awd
{
    public class AddFileViewModel : ViewModelBase
    {
        public IEnumerable<FileType> FileTypes { get; } = 
            Enumerable.Range(0x4086, 25).Select(i => new FileType("PCI", i))
            .Union(Enumerable.Range(0x40A0, 7).Select(i => new FileType("ISA", i)));


        string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        FileType fileType;
        public FileType FileType
        {
            get { return fileType; }
            set
            {
                if (fileType != value)
                {
                    fileType = value;
                    RaisePropertyChanged();
                }
            }
        }

        string fixedOffset;
        public string FixedOffset
        {
            get { return fixedOffset; }
            set
            {
                if (fixedOffset != value)
                {
                    fixedOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand BrowseFileCommand => new RelayCommand(o =>
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Select file to add",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "All files|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                FilePath = dlg.FileName;
            }
        });

        public Action<bool> CloseAction { get; set; }

        public ICommand OkCommand => new RelayCommand(o =>
        {
            if (FileType == null || !File.Exists(FilePath) || !IsValidHex(fixedOffset))
                return;
            this.CloseAction(true);
        }, o => FileType != null && File.Exists(FilePath) && IsValidHex(fixedOffset));

        public ICommand CancelCommand => new RelayCommand(o =>
        {
            this.CloseAction(false);
        });

        private bool IsValidHex(string fixedOffset)
        {
            var hexaCode = new Regex(@"^#([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");
            return string.IsNullOrWhiteSpace(fixedOffset) || hexaCode.IsMatch(fixedOffset);
        }
    }
}
