using Awd.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awd
{
    public class BiosViewModel : ViewModelBase
    {
        public BiosViewModel(BiosImage bios)
        {
            Bios = bios;
            Log.Add($"This image {bios.FileName} appears to be a valid {bios.ImageSize} bytes Award BIOS image.");

        }

        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();
        public BiosImage Bios { get; }

        public IBiosModule SelectedFile { get; set; }

        internal void Load()
        {
            Bios.Load();
            Log.Add($"{Bios.ImageVersion} File table Offset 0x{Bios.TableOffset:X} - Layout {Bios.TableLayout} - Checksum Seed {Bios.ChecksumSeed:X}");

        }

        internal void Refresh()
        {
            RaisePropertyChanged(null);
            foreach (var module in Bios.ModulesView)
                module.Refresh();
        }
    }
}
