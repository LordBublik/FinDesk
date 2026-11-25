using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinDesk.Models
{
    public class CategoryItem : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Type { get; set; } = "expense";
        public string TypeRu => Type == "income" ? "Доход" : "Расход";
        public bool IsUsed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}