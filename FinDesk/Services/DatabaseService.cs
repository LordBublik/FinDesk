using FinDesk.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FinDesk.Services
{
    public class DatabaseService
    {
        private FinDeskKursovoiContext CreateContext()
        {
            return new FinDeskKursovoiContext();
        }

        public List<Transaction> GetAllTransactions()
        {
            using var context = CreateContext();
            return context.Transactions
                          .Include(t => t.Category)
                          .OrderByDescending(t => t.Date)
                          .ToList();
        }

        public List<Category> GetAllCategories()
        {
            using var context = CreateContext();
            return context.Categories.ToList();
        }

        public void AddTransaction(decimal amount, string type, string description, int categoryId, DateTime date)
        {
            using var context = CreateContext();

            var newTransaction = new Transaction
            {
                Amount = amount,
                Type = type,
                Description = description,
                CategoryId = categoryId,
                Date = date
            };

            context.Transactions.Add(newTransaction);
            context.SaveChanges();
        }

        public void DeleteTransaction(int id)
        {
            using var context = CreateContext();
            var transaction = context.Transactions.Find(id);

            if (transaction != null)
            {
                context.Transactions.Remove(transaction);
                context.SaveChanges();
            }
        }

        public decimal GetBalance()
        {
            using var context = CreateContext();

            decimal totalIncome = context.Transactions
                .Where(t => t.Type == "income")
                .Sum(t => t.Amount);

            decimal totalExpense = context.Transactions
                .Where(t => t.Type == "expense")
                .Sum(t => t.Amount);

            return totalIncome - totalExpense;
        }

        public string GetSetting(string key)
        {
            using var context = CreateContext();
            using var connection = context.Database.GetDbConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT value FROM settings WHERE key = @key";

            var param = command.CreateParameter();
            param.ParameterName = "@key";
            param.Value = key;
            command.Parameters.Add(param);

            var result = command.ExecuteScalar();
            return result?.ToString() ?? "light";
        }

        public void SaveSetting(string key, string value)
        {
            using var context = CreateContext();
            using var connection = context.Database.GetDbConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO settings (key, value) 
                VALUES (@key, @value) 
                ON CONFLICT (key) 
                DO UPDATE SET value = @value, updated_at = CURRENT_TIMESTAMP";

            var paramKey = command.CreateParameter();
            paramKey.ParameterName = "@key";
            paramKey.Value = key;
            command.Parameters.Add(paramKey);

            var paramValue = command.CreateParameter();
            paramValue.ParameterName = "@value";
            paramValue.Value = value;
            command.Parameters.Add(paramValue);

            command.ExecuteNonQuery();
        }

        public List<Transaction> GetFilteredTransactions(DateTime? fromDate, DateTime? toDate, string searchQuery)
        {
            using var context = CreateContext();

            var query = context.Transactions.Include(t => t.Category).AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1);
                query = query.Where(t => t.Date < endOfDay);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(t => t.Description != null && t.Description.ToLower().Contains(lowerQuery));
            }

            return query.OrderByDescending(t => t.Date).ToList();
        }

        public void AddCategory(string name, string type)
        {
            using var context = CreateContext();

            var newCategory = new Category
            {
                Name = name,
                Type = type
            };

            context.Categories.Add(newCategory);
            context.SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            using var context = CreateContext();
            var category = context.Categories.Find(id);

            if (category != null)
            {
                context.Categories.Remove(category);
                context.SaveChanges();
            }
        }

        public bool IsCategoryUsed(int categoryId)
        {
            using var context = CreateContext();
            return context.Transactions.Any(t => t.CategoryId == categoryId);
        }

        public bool CategoryExists(string name, string type, int? excludeId = null)
        {
            using var context = CreateContext();
            var query = context.Categories.Where(c => c.Name == name && c.Type == type);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return query.Any();
        }
    }
}