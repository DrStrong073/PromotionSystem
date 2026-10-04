using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;

namespace WebApi.Extensions
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            // Tạo một scope để lấy các service đã đăng ký trong DI Container
            using var scope = serviceProvider.CreateScope();

            // Lấy MongoDB và Elasticsearch service ra
            var mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var esService = scope.ServiceProvider.GetRequiredService<IElasticSearchService<Voucher>>();
            var esPromoService = scope.ServiceProvider.GetRequiredService<IElasticSearchService<Promotion>>();

            // Trỏ tới các collection
            var promotionCollection = mongoDb.GetCollection<Promotion>("Promotions");
            var voucherCollection = mongoDb.GetCollection<Voucher>("Vouchers");

            // Kiểm tra xem đã có Promotion nào trong Database chưa
            var promoCount = await promotionCollection.CountDocumentsAsync(FilterDefinition<Promotion>.Empty);

            // Nếu database hoàn toàn trống -> Bắt đầu tạo dữ liệu mẫu
            if (promoCount == 0)
            {
                Console.WriteLine("Seeding Data...");

                // 1. TẠO 2 CHƯƠNG TRÌNH KHUYẾN MÃI
                var promo1 = new Promotion
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Black Friday 2026",
                    Description = "Siêu sale cuối năm 2026",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(15),
                    IsActive = true
                };

                var promo2 = new Promotion
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Mừng Xuân 2027",
                    Description = "Khuyến mãi tết nguyên đán",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    IsActive = true
                };
                var promotions = new[] { promo1, promo2 };
                // Lưu Promotions vào MongoDB
                await promotionCollection.InsertManyAsync(new[] { promo1, promo2 });
                await esPromoService.BulkIndexAsync(promotions);
                // 2. TẠO HÀNG LOẠT VOUCHER (Mỗi chương trình 50 mã = 100 mã)
                var vouchers = new List<Voucher>();
                var random = new Random();

                // Tạo mã cho Black Friday
                for (int i = 1; i <= 50; i++)
                {
                    vouchers.Add(new Voucher
                    {
                        Id = Guid.NewGuid().ToString(),
                        PromotionId = promo1.Id,
                        Code = $"BF26-{i:000}-{random.Next(1000, 9999)}", // VD: BF26-001-4829
                        DiscountAmount = random.Next(10, 50) * 1000, // Random giảm từ 10k đến 50k
                        MinOrderValue = 100000, // Đơn tối thiểu 100k
                        UsageLimit = 100,
                        UsedCount = random.Next(0, 10), // Đã dùng vài lần
                        IsActive = true
                    });
                }

                // Tạo mã cho Tết
                for (int i = 1; i <= 50; i++)
                {
                    vouchers.Add(new Voucher
                    {
                        Id = Guid.NewGuid().ToString(),
                        PromotionId = promo2.Id,
                        Code = $"TET27-{i:000}-{random.Next(1000, 9999)}",
                        DiscountAmount = random.Next(50, 100) * 1000, // Random giảm từ 50k đến 100k
                        MinOrderValue = 300000, // Đơn tối thiểu 300k
                        UsageLimit = 50,
                        UsedCount = 0,
                        IsActive = true
                    });
                }

                // Lưu Vouchers vào MongoDB
                await voucherCollection.InsertManyAsync(vouchers);

                // ĐỒNG BỘ Vouchers sang Elasticsearch (Dùng Bulk để chèn 100 mã cùng lúc)
                await esService.BulkIndexAsync(vouchers);

                Console.WriteLine("Seeding complete! Created 2 promotions and 100 vouchers.");
            }
        }
    }
}
