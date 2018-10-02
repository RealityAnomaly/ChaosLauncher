using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class Tool : INotifyPropertyChanged
    {
        private string name;
        private string nickname;
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Nickname { get { return nickname; } set { nickname = value; OnPropertyChanged("Nickname"); } }
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public readonly ObservableCollection<Parameter> Parameters = new ObservableCollection<Parameter>();
        public readonly ObservableCollection<Output> Outputs = new ObservableCollection<Output>();

        public event PropertyChangedEventHandler PropertyChanged;

        public Tool(string name)
        {
            Name = name;
            Nickname = GetShortName();
            Outputs.Add(new Output(null));
        }

        public override string ToString()
        {
            return Nickname;
        }

        public virtual string GetShortName()
        {
            return Name;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
