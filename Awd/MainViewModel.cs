
using Awd.MVVM;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Awd
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly string fileName;
        public MainViewModel(string fileName)
        {
            this.fileName = fileName;
        }

        public ObservableCollection<BiosViewModel> Files { get; } = new ObservableCollection<BiosViewModel>();

        public ICommand ExitCommand => new Awd.MVVM.RelayCommand(_ => System.Windows.Application.Current.Shutdown());

        public ICommand LoadCommand => new Awd.MVVM.RelayCommand(async _ => await Load(fileName));
        public ICommand OpenCommand => new Awd.MVVM.RelayCommand(async _ =>
        {
            var openFile = new OpenFileDialog();
            openFile.Multiselect = false;

            if (openFile.ShowDialog().GetValueOrDefault())
            {
                var fileName = openFile.FileName;
                await Load(fileName);
            }
        });

        public ICommand ReplaceFileCommand => new Awd.MVVM.RelayCommand<BiosViewModel>(bios =>
        {
            if (bios.SelectedFile is LzhModule lzhModule)
            {
                var openFile = new OpenFileDialog();
                openFile.Multiselect = false;

                if (openFile.ShowDialog().GetValueOrDefault())
                {
                    var fileName = openFile.FileName;

                    lzhModule.Replace(fileName);
                    bios.Refresh();
                }
            }
        }, bios => bios?.SelectedFile is LzhModule);

        public ICommand AddFileCommand => new Awd.MVVM.RelayCommand<BiosViewModel>(bios =>
        {
            if (bios.SelectedFile is LzhModule lzhModule)
            {
                var openFile = new OpenFileDialog();
                openFile.Multiselect = false;

                if (openFile.ShowDialog().GetValueOrDefault())
                {
                    var fileName = openFile.FileName;
                    var itemBefore = bios.Bios.ModulesView.OfType<LzhModule>().LastOrDefault(m => !m.IsFixedOffset);
                    var index = bios.Bios.ModulesView.IndexOf(itemBefore) + 1;
                    var newModule = new LzhModule();
                    newModule.Replace(fileName);
                    bios.Bios.ModulesView.Insert(index, newModule);
                    bios.Refresh();
                }
            }
        });

        public ICommand ExtractFileCommand => new Awd.MVVM.RelayCommand<BiosViewModel>(bios =>
        {
            if (bios.SelectedFile is IBiosModule biosModule)
            {
                var openFile = new SaveFileDialog();

                if (openFile.ShowDialog().GetValueOrDefault())
                {
                    var fileName = openFile.FileName;
                    File.WriteAllBytes(fileName, biosModule.Decompressed);
                }
            }
        }, bios => bios?.SelectedFile is IBiosModule);

        public ICommand SaveAsCommand => new Awd.MVVM.RelayCommand<BiosViewModel>(async vm =>
        {
            if (vm == null)
                return;
            var saveFile = new SaveFileDialog();
            saveFile.FileName = vm.Bios.FileName;
            if (saveFile.ShowDialog().GetValueOrDefault())
            {
                var fileName = saveFile.FileName;
                this.Overlay = new WaitViewModel() { Message = "Saving BIOS image..." };
                try
                {
                    await Dispatcher.CurrentDispatcher.InvokeAsync(() => vm.Bios.Save(fileName, false));
                }
                finally
                {
                    this.Overlay = null;
                }
            }
        });

        public ICommand CloseFileCommand => new Awd.MVVM.RelayCommand<BiosViewModel>(async vm =>
        {
            if (vm == null)
                return;

            this.Files.Remove(vm);
        });

        internal async Task Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            if (!File.Exists(fileName))
            {
                MessageBox.Show("File not found");
                return;
            }
            var bios = new Awd.BiosImage(await File.ReadAllBytesAsync(fileName), fileName);
            if (!bios.IsValid)
            {
                MessageBox.Show("This image does not appear to be a valid Award BIOS image.");
                return;
            }
            var viewModel = new BiosViewModel(bios);
            this.Files.Add(viewModel);

            this.Overlay = new WaitViewModel() { Message = "Loading BIOS image..." };
            try
            {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => viewModel.Load());
            }
            finally
            {
                this.Overlay = null;
            }
        }

        ViewModelBase overlay;
        public ViewModelBase Overlay
        {
            get { return overlay; }
            set
            {
                if (overlay != value)
                {
                    overlay = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}