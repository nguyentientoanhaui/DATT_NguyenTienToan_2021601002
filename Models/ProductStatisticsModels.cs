using System;
using System.Collections.Generic;

namespace Shopping_Demo.Models
{
    // Main model for product statistics
    public class ProductStatisticsModel
    {
        public ProductStatisticsModel()
        {
            InventoryDistribution = new InventoryDistributionModel();
            LowStockProducts = new List<LowStockProductModel>();
            TopSellingProducts = new List<ProductSalesModel>();
            WorstSellingProducts = new List<ProductSalesModel>();
            HighProfitMarginProducts = new List<ProfitMarginProductModel>();
        }

        public InventoryDistributionModel InventoryDistribution { get; set; }
        public List<LowStockProductModel> LowStockProducts { get; set; }
        public List<ProductSalesModel> TopSellingProducts { get; set; }
        public List<ProductSalesModel> WorstSellingProducts { get; set; }
        public List<ProfitMarginProductModel> HighProfitMarginProducts { get; set; }
    }

    // Model for inventory distribution
    public class InventoryDistributionModel
    {
        public InventoryDistributionModel()
        {
            CategoryDistribution = new List<DistributionItemModel>();
            BrandDistribution = new List<DistributionItemModel>();
        }

        public int TotalInventoryQuantity { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<DistributionItemModel> CategoryDistribution { get; set; }
        public List<DistributionItemModel> BrandDistribution { get; set; }
    }

    // Model for distribution items (category or brand)
    public class DistributionItemModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public double Percentage { get; set; }
    }

    // Model for low stock products
    public class LowStockProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsUrgent => StockQuantity <= ReorderLevel;
    }

    // Model for product sales data
    public class ProductSalesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SoldQuantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    // Model for profit margin products
    public class ProfitMarginProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public double ProfitMargin { get; set; }
        public int SoldQuantity { get; set; }
        public decimal TotalProfit { get; set; }
    }

    // Model for category performance
    public class CategoryPerformanceModel
    {
        public string Name { get; set; }
        public int ProductCount { get; set; }
        public int InventoryQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    // Overall product performance metrics
    public class ProductPerformanceOverviewModel
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalInventory { get; set; }
        public decimal InventoryValue { get; set; }
        public int SoldItems { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public double AverageProfitMargin { get; set; }
    }
}