using System;
using System.Collections.Generic;
using System.Linq;

public class Currency
{
    public string Symbol { get; }

    public Currency(string symbol)
    {
        Symbol = symbol;
    }
}

public class VendingMachine
{
    private VendingMachineBank bank;
    private StockManager stockManager;
    private Currency currentCurrency;

    public VendingMachine(VendingMachineBank bank, StockManager stockManager, Currency currentCurrency)
    {
        this.bank = bank;
        this.stockManager = stockManager;
        this.currentCurrency = currentCurrency;
    }

    public void DisplayStock()
    {
        Console.WriteLine("Available items:");
        foreach (var item in stockManager.Stock)
        {
            Console.WriteLine($"{item.Id}. {item.Name} - {currentCurrency.Symbol}{item.Price:F2}");
        }
    }

    public void SelectItem(int itemId)
    {
        var selectedItem = stockManager.Stock.Find(item => item.Id == itemId);
        if (selectedItem != null)
        {
            Console.WriteLine($"You have selected: {selectedItem.Name} - {currentCurrency.Symbol}{selectedItem.Price:F2}");

            Console.Write($"Enter how you are paying (e.g., '{currentCurrency.Symbol}1.50', '{currentCurrency.Symbol}2.00', etc.): ");
            if (decimal.TryParse(Console.ReadLine().Replace(currentCurrency.Symbol, ""), out decimal paymentAmount))
            {
                if (paymentAmount >= selectedItem.Price)
                {
                    decimal change = bank.GiveChange(selectedItem.Price, paymentAmount);
                    Console.WriteLine($"Payment successful. Change: {currentCurrency.Symbol}{change:F2}");
                }
                else
                {
                    Console.WriteLine("Insufficient funds. Please insert more coins.");
                }
            }
            else
            {
                Console.WriteLine("Invalid payment amount. Please enter a valid decimal value.");
            }
        }
        else
        {
            Console.WriteLine("Invalid item selection.");
        }
    }

    public void AddStockItem(int id, string name, decimal price)
    {
        stockManager.AddStockItem(id, name, price);
        Console.WriteLine($"Added new item: {name} - {currentCurrency.Symbol}{price:F2}");
    }

    public void RemoveStockItem(int id)
    {
        if (stockManager.RemoveStockItem(id))
        {
            Console.WriteLine($"Removed item with ID {id} from stock.");
        }
        else
        {
            Console.WriteLine($"Item with ID {id} not found in stock.");
        }
    }
}

public class StockManager
{
    public List<VendingMachineItem> Stock { get; }

    public StockManager(List<VendingMachineItem> initialStock)
    {
        Stock = initialStock;
    }

    public void AddStockItem(int id, string name, decimal price)
    {
        var newItem = new VendingMachineItem(id, name, price);
        Stock.Add(newItem);
    }

    public bool RemoveStockItem(int id)
    {
        var itemToRemove = Stock.FirstOrDefault(item => item.Id == id);
        if (itemToRemove != null)
        {
            Stock.Remove(itemToRemove);
            return true;
        }
        return false;
    }
}

public class VendingMachineBank
{
    private decimal amount;
    private Dictionary<decimal, int> coinDenominations;
    private Currency currentCurrency;

    public VendingMachineBank(decimal initialAmount, Currency currentCurrency)
    {
        amount = initialAmount;
        this.currentCurrency = currentCurrency;
        InitializeCoinDenominations();
    }

    private void InitializeCoinDenominations()
    {
        coinDenominations = new Dictionary<decimal, int>
        {
            { 2.00m, 10 }, // £2 coins
            { 1.00m, 10 }, // £1 coins
            { 0.50m, 10 }, // 50p coins
            { 0.20m, 10 }, // 20p coins
            { 0.10m, 10 }, // 10p coins
            { 0.05m, 10 }, // 5p coins
            { 0.02m, 10 }, // 2p coins
            { 0.01m, 10 }  // 1p coins
        };
    }

    public decimal GiveChange(decimal itemPrice, decimal paymentAmount)
    {
        decimal changeAmount = paymentAmount - itemPrice;

        if (changeAmount > 0)
        {
            Console.WriteLine($"Change given: {currentCurrency.Symbol}{changeAmount:F2}");
            DispenseChange(changeAmount);
        }

        amount += itemPrice;
        return changeAmount;
    }

    private void DispenseChange(decimal changeAmount)
    {
        foreach (var denomination in coinDenominations.OrderByDescending(d => d.Key))
        {
            int coinsToDispense = (int)(changeAmount / denomination.Key);
            if (coinsToDispense > 0 && coinDenominations[denomination.Key] >= coinsToDispense)
            {
                Console.WriteLine($"Dispensing {coinsToDispense} x {currentCurrency.Symbol}{denomination.Key:F2} coins");
                changeAmount -= coinsToDispense * denomination.Key;
                coinDenominations[denomination.Key] -= coinsToDispense;
            }
        }
    }
}

public class VendingMachineItem
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }

    public VendingMachineItem(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("================================");
        Console.WriteLine("Welcome to the Vending Machine!");
        Console.WriteLine("================================");

        Console.Write("Enter the currency symbol: ");
        string currencySymbol = Console.ReadLine();

        Currency selectedCurrency = new Currency(currencySymbol);

        VendingMachineBank bank = new VendingMachineBank(0, selectedCurrency);
        List<VendingMachineItem> initialStock = new List<VendingMachineItem>
        {
            new VendingMachineItem(1, "Cola", 1.50m),
            new VendingMachineItem(2, "Crisps", 1.25m),
            new VendingMachineItem(3, "Sprite", 1.00m)
        };

        StockManager stockManager = new StockManager(initialStock);
        VendingMachine vendingMachine = new VendingMachine(bank, stockManager, selectedCurrency);

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. Choose an item");
            Console.WriteLine("2. Add stock");
            Console.WriteLine("3. Remove stock");
            Console.WriteLine("4. Exit");

            Console.Write("Enter your choice (1-4): ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        vendingMachine.DisplayStock();
                        Console.Write("Enter the item number you want to purchase: ");
                        if (int.TryParse(Console.ReadLine(), out int itemId))
                        {
                            vendingMachine.SelectItem(itemId);
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid item number.");
                        }
                        break;
                    case 2:
                        Console.WriteLine("\nAdding a new stock item:");
                        Console.Write("Enter item ID: ");
                        int newId = int.Parse(Console.ReadLine());
                        Console.Write("Enter item name: ");
                        string newName = Console.ReadLine();
                        Console.Write($"Enter item price ({selectedCurrency.Symbol}): ");
                        decimal newPrice = decimal.Parse(Console.ReadLine());

                        vendingMachine.AddStockItem(newId, newName, newPrice);

                        Console.WriteLine("\nUpdated stock:");
                        vendingMachine.DisplayStock();
                        break;
                    case 3:
                        Console.WriteLine("\nRemoving stock item:");
                        Console.Write("Enter item ID to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int removeItemId))
                        {
                            vendingMachine.RemoveStockItem(removeItemId);
                            Console.WriteLine("\nUpdated stock:");
                            vendingMachine.DisplayStock();
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid item number.");
                        }
                        break;
                    case 4:
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter a number between 1 and 4.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
            }
        }
    }
}
