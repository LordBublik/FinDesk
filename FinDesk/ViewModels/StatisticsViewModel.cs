using FinDesk.Models;
using FinDesk.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FinDesk.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        private decimal _totalIncome;
        public decimal TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(); }
        }

        private decimal _totalExpense;
        public decimal TotalExpense
        {
            get => _totalExpense;
            set { _totalExpense = value; OnPropertyChanged(); }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        private int _transactionCount;
        public int TransactionCount
        {
            get => _transactionCount;
            set { _transactionCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryStatistic> CategoryStatistics { get; set; }

        public ObservableCollection<DailyStatistic> DailyStatistics { get; set; }

        private decimal _maxDailyAmount;
        public decimal MaxDailyAmount
        {
            get => _maxDailyAmount;
            set { _maxDailyAmount = value; OnPropertyChanged(); }
        }

        public StatisticsViewModel()
        {
            _dbService = new DatabaseService();
            CategoryStatistics = new ObservableCollection<CategoryStatistic>();
            DailyStatistics = new ObservableCollection<DailyStatistic>();
            LoadStatistics();
        }

        public void Refresh()
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            var transactions = _dbService.GetAllTransactions();

            TotalIncome = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);
            Balance = TotalIncome - TotalExpense;
            TransactionCount = transactions.Count;

            CategoryStatistics.Clear();
            var categoryStats = transactions
                .Where(t => t.Type == "expense")
                .GroupBy(t => t.Category?.Name ?? "Без категории")
                .Select(g => new CategoryStatistic
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Percentage = TotalExpense > 0
                        ? (double)((g.Sum(t => t.Amount) / TotalExpense) * 100)
                        : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            foreach (var stat in categoryStats)
            {
                CategoryStatistics.Add(stat);
            }

            DailyStatistics.Clear();
            var dailyStats = transactions
                .GroupBy(t => t.Date.Date)
                .Select(g => new DailyStatistic
                {
                    DateLabel = g.Key.ToString("dd.MM"),
                    Income = g.Where(t => t.Type == "income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == "expense").Sum(t => t.Amount)
                })
                .OrderBy(d => d.DateLabel)
                .ToList();

            MaxDailyAmount = dailyStats.Any()
                ? dailyStats.Max(d => Math.Max(d.Income, d.Expense))
                : 1;

            foreach (var stat in dailyStats)
            {
                DailyStatistics.Add(stat);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}