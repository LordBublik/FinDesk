using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FinDesk.Models;
using FinDesk.Services;
using FinDesk.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinDesk.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly Window _window;

        private decimal _balance;
        public decimal Balance { get => _balance; set { _balance = value; OnPropertyChanged(); } }

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions { get => _transactions; set { _transactions = value; OnPropertyChanged(); } }

        private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories { get => _categories; set { _categories = value; OnPropertyChanged(); } }

        private decimal _newAmount;
        public decimal NewAmount { get => _newAmount; set { _newAmount = value; OnPropertyChanged(); } }

        private string _newDescription = "";
        public string NewDescription { get => _newDescription; set { _newDescription = value; OnPropertyChanged(); } }

        private string _selectedType = "Расход";
        public string SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); FilterCategories(); }
        }

        private bool _isExpenseSelected = true;
        public bool IsExpenseSelected
        {
            get => _isExpenseSelected;
            set
            {
                _isExpenseSelected = value;
                if (value) { SelectedType = "Расход"; IsIncomeSelected = false; }
                OnPropertyChanged();
            }
        }

        private bool _isIncomeSelected = false;
        public bool IsIncomeSelected
        {
            get => _isIncomeSelected;
            set
            {
                _isIncomeSelected = value;
                if (value) { SelectedType = "Доход"; IsExpenseSelected = false; }
                OnPropertyChanged();
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private string _errorMessage = "";
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _hasError;
        public bool HasError { get => _hasError; set { _hasError = value; OnPropertyChanged(); } }

        private DateTimeOffset? _filterFromDate;
        public DateTimeOffset? FilterFromDate { get => _filterFromDate; set { _filterFromDate = value; OnPropertyChanged(); } }

        private DateTimeOffset? _filterToDate;
        public DateTimeOffset? FilterToDate { get => _filterToDate; set { _filterToDate = value; OnPropertyChanged(); } }

        private string _searchQuery = "";
        public string SearchQuery { get => _searchQuery; set { _searchQuery = value; OnPropertyChanged(); } }

        private bool _isFilterActive;
        public bool IsFilterActive { get => _isFilterActive; set { _isFilterActive = value; OnPropertyChanged(); } }

        public ICommand AddTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand ExportCommand { get; }

        public MainViewModel(Window window)
        {
            _window = window;
            _dbService = new DatabaseService();
            Transactions = new ObservableCollection<Transaction>();
            Categories = new ObservableCollection<Category>();

            AddTransactionCommand = new RelayCommand(AddTransaction);
            DeleteTransactionCommand = new RelayCommand<int>(DeleteTransaction);
            RefreshCommand = new RelayCommand(LoadData);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            ExportCommand = new RelayCommand(ExportToCsv);

            LoadData();
        }

        private void FilterCategories()
        {
            var allCategories = _dbService.GetAllCategories();
            Categories.Clear();
            var filteredCategories = allCategories.Where(c => c.Type == (IsExpenseSelected ? "expense" : "income")).ToList();
            foreach (var category in filteredCategories) Categories.Add(category);
            SelectedCategory = Categories.FirstOrDefault();
        }

        public void LoadData()
        {
            ErrorMessage = ""; HasError = false;
            try
            {
                Balance = _dbService.GetBalance();
                Transactions.Clear();
                foreach (var t in _dbService.GetAllTransactions()) Transactions.Add(t);
                FilterCategories();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке данных: " + ex.Message;
                HasError = true;
            }
        }

        private void AddTransaction()
        {
            ErrorMessage = ""; HasError = false;
            if (NewAmount <= 0) { ErrorMessage = "Сумма должна быть больше нуля"; HasError = true; return; }
            if (SelectedCategory == null) { ErrorMessage = "Пожалуйста, выберите категорию"; HasError = true; return; }
            if (string.IsNullOrEmpty(SelectedType)) { ErrorMessage = "Пожалуйста, выберите тип операции"; HasError = true; return; }

            try
            {
                _dbService.AddTransaction(NewAmount, IsExpenseSelected ? "expense" : "income", NewDescription, SelectedCategory.Id, DateTime.Now);
                NewAmount = 0; NewDescription = ""; SelectedCategory = null;
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при сохранении: " + ex.Message; HasError = true;
            }
        }

        private async void DeleteTransaction(int transactionId)
        {
            var confirmDialog = new ConfirmationView("Вы уверены, что хотите удалить эту запись? Это действие нельзя отменить.");
            await confirmDialog.ShowDialog(_window);

            if (confirmDialog.Result)
            {
                try
                {
                    _dbService.DeleteTransaction(transactionId);
                    LoadData();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Ошибка при удалении: " + ex.Message;
                    HasError = true;
                }
            }
        }

        private void ApplyFilter()
        {
            ErrorMessage = ""; HasError = false;
            try
            {
                if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate > FilterToDate)
                {
                    ErrorMessage = "Дата начала не может быть позже даты окончания"; HasError = true; return;
                }
                var filteredTransactions = _dbService.GetFilteredTransactions(FilterFromDate?.DateTime, FilterToDate?.DateTime, SearchQuery);
                Transactions.Clear();
                foreach (var t in filteredTransactions) Transactions.Add(t);
                IsFilterActive = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при фильтрации: " + ex.Message; HasError = true;
            }
        }

        private void ClearFilter()
        {
            FilterFromDate = null; FilterToDate = null; SearchQuery = ""; IsFilterActive = false;
            LoadData();
        }

        public async void ExportToCsv()
        {
            try
            {
                List<Transaction> transactionsToExport = IsFilterActive
                    ? _dbService.GetFilteredTransactions(FilterFromDate?.DateTime, FilterToDate?.DateTime, SearchQuery)
                    : _dbService.GetAllTransactions();

                if (transactionsToExport.Count == 0) { ErrorMessage = "Нет данных для экспорта!"; HasError = true; return; }

                var csvLines = new List<string> { "ID;Дата;Тип;Сумма;Категория;Описание" };
                foreach (var t in transactionsToExport)
                {
                    var typeRu = t.Type == "income" ? "Доход" : "Расход";
                    var category = t.Category?.Name ?? "Без категории";
                    var description = t.Description?.Replace(";", ",") ?? "";
                    csvLines.Add($"{t.Id};{t.Date:dd.MM.yyyy HH:mm};{typeRu};{t.Amount};{category};{description}");
                }

                var storageProvider = _window.StorageProvider;
                var file = await storageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Экспорт транзакций",
                    SuggestedFileName = $"transactions_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv",
                    DefaultExtension = "csv",
                    FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("CSV файлы") { Patterns = new[] { "*.csv" } } }
                });

                if (file != null)
                {
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    await writer.WriteAsync(string.Join("\n", csvLines));
                    ErrorMessage = $"Успешно экспортировано {transactionsToExport.Count} транзакций"; HasError = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при экспорте: " + ex.Message; HasError = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged;
    }
}