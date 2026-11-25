using FinDesk.Models;
using FinDesk.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FinDesk.ViewModels
{
    public class CategoryManagerViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<CategoryItem> Categories { get; set; }

        private string _newCategoryName = "";
        public string NewCategoryName
        {
            get => _newCategoryName;
            set { _newCategoryName = value; OnPropertyChanged(); }
        }

        private bool _isExpenseType = true;
        public bool IsExpenseType
        {
            get => _isExpenseType;
            set
            {
                _isExpenseType = value;
                if (value)
                {
                    NewCategoryType = "expense";
                    IsIncomeType = false;
                }
                OnPropertyChanged();
            }
        }

        private bool _isIncomeType = false;
        public bool IsIncomeType
        {
            get => _isIncomeType;
            set
            {
                _isIncomeType = value;
                if (value)
                {
                    NewCategoryType = "income";
                    IsExpenseType = false;
                }
                OnPropertyChanged();
            }
        }

        private string _newCategoryType = "expense";
        public string NewCategoryType
        {
            get => _newCategoryType;
            set { _newCategoryType = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        private string _successMessage = "";
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action<bool> Closed;

        public CategoryManagerViewModel()
        {
            _dbService = new DatabaseService();
            Categories = new ObservableCollection<CategoryItem>();

            AddCategoryCommand = new RelayCommand(AddCategory);
            DeleteCategoryCommand = new RelayCommand<int>(DeleteCategory);
            CloseCommand = new RelayCommand(() => Closed?.Invoke(true));

            LoadCategories();
        }

        private void LoadCategories()
        {
            var allCategories = _dbService.GetAllCategories();

            Categories.Clear();
            foreach (var c in allCategories)
            {
                Categories.Add(new CategoryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    IsUsed = _dbService.IsCategoryUsed(c.Id)
                });
            }
        }

        private void AddCategory()
        {
            ErrorMessage = "";
            HasError = false;
            SuccessMessage = "";

            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                ErrorMessage = "Введите название категории";
                HasError = true;
                return;
            }

            if (NewCategoryName.Length > 50)
            {
                ErrorMessage = "Название не должно превышать 50 символов";
                HasError = true;
                return;
            }

            if (NewCategoryType != "expense" && NewCategoryType != "income")
            {
                ErrorMessage = "Не выбран тип категории";
                HasError = true;
                return;
            }

            if (_dbService.CategoryExists(NewCategoryName.Trim(), NewCategoryType))
            {
                ErrorMessage = "Такая категория уже существует";
                HasError = true;
                return;
            }

            try
            {
                _dbService.AddCategory(NewCategoryName.Trim(), NewCategoryType);
                SuccessMessage = $"Категория '{NewCategoryName.Trim()}' добавлена";
                NewCategoryName = "";
                LoadCategories();
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\n" + ex.InnerException.Message;
                }
                ErrorMessage = "Ошибка при добавлении: " + errorMsg;
                HasError = true;
            }
        }

        private void DeleteCategory(int categoryId)
        {
            ErrorMessage = "";
            HasError = false;
            SuccessMessage = "";

            var category = Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (category.IsUsed)
            {
                ErrorMessage = $"Нельзя удалить категорию '{category.Name}' — она используется в транзакциях";
                HasError = true;
                return;
            }

            try
            {
                _dbService.DeleteCategory(categoryId);
                SuccessMessage = $"Категория '{category.Name}' удалена";
                LoadCategories();
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\n" + ex.InnerException.Message;
                }
                ErrorMessage = "Ошибка при удалении: " + errorMsg;
                HasError = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}