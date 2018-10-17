using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SourceLauncher.Models
{
    public abstract class Tool : INotifyPropertyChanged
    {
        private string _name;
        private string _nickname;
        public string Name { get => _name; set { _name = value; OnPropertyChanged("Name"); } }
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged("Nickname"); } }
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public readonly ObservableCollection<Parameter> Parameters = new ObservableCollection<Parameter>();
        public readonly ObservableCollection<Output> Outputs = new ObservableCollection<Output>();

        public event PropertyChangedEventHandler PropertyChanged;

        internal Tool(string name)
        {
            Name = name;
            Nickname = GetShortName();
            Outputs.Add(new Output());
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
