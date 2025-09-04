using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Microsoft.AspNetCore.Identity;
using Shopping_Demo.Models.ViewModels;

namespace Shopping_Demo.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			
			// Create admin user and roles if they don't exist
			CreateAdminUserAndRoles(_context);
			
			if (!_context.Products.Any())
			{
				// Categories for watches
				CategoryModel milgauss = new CategoryModel { Name = "Milgauss", Slug = "milgauss", Description = "Rolex Milgauss Collection", Status = 1, Level = 1 };
				CategoryModel yachtMaster = new CategoryModel { Name = "Yacht-Master 40", Slug = "yacht-master-40", Description = "Rolex Yacht-Master 40 Collection", Status = 1, Level = 1 };
				CategoryModel submariner = new CategoryModel { Name = "Submariner", Slug = "submariner", Description = "Rolex Submariner Collection", Status = 1, Level = 1 };
				CategoryModel date = new CategoryModel { Name = "Date", Slug = "date", Description = "Rolex Date Collection", Status = 1, Level = 1 };
				CategoryModel oysterPerpetual = new CategoryModel { Name = "Oyster Perpetual 34", Slug = "oyster-perpetual-34", Description = "Rolex Oyster Perpetual 34 Collection", Status = 1, Level = 1 };

				// Brands
				BrandModel rolex = new BrandModel { Name = "Rolex", Slug = "rolex", Description = "Luxury Swiss watch manufacturer", Status = 1 };
				BrandModel omega = new BrandModel { Name = "Omega", Slug = "omega", Description = "Swiss luxury watchmaker", Status = 1 };
				BrandModel panerai = new BrandModel { Name = "Panerai", Slug = "panerai", Description = "Italian luxury watch manufacturer", Status = 1 };

				_context.Categories.AddRange(milgauss, yachtMaster, submariner, date, oysterPerpetual);
				_context.Brands.AddRange(rolex, omega, panerai);
				_context.SaveChanges();

				_context.Products.AddRange(
					new ProductModel {
                        Name = "Rolex Milgauss 116400GV",
                        Slug = "rolex-milgauss-116400gv",
                        Description = "Rolex Milgauss with green sapphire crystal and lightning bolt second hand. Anti-magnetic watch for scientists.",
                        Price = 8950,
                        CapitalPrice = 6500,
                        BrandId = rolex.Id,
                        CategoryId = milgauss.Id,
                        Quantity = 3,
                        Sold = 1,
                        Image = "ab77a7b3-61d8-442a-af87-a915ff45a71a_IE8976-1.webp",
                        IsActive = true,
                        Model = "Milgauss",
                        ModelNumber = "116400GV",
                        Gender = "Men",
                        Condition = "Excellent",
                        CaseMaterial = "Stainless Steel",
                        CaseSize = "40mm",
                        Crystal = "Green Sapphire",
                        DialColor = "Black",
                        MovementType = "Automatic",
                        BraceletMaterial = "Stainless Steel"
                    },
					new ProductModel {
                        Name = "Rolex Yacht-Master 40 126622",
                        Slug = "rolex-yacht-master-126622",
                        Description = "Rolex Yacht-Master with blue dial and platinum bezel. Luxury sports watch with sailing heritage.",
                        Price = 12500,
                        CapitalPrice = 9000,
                        BrandId = rolex.Id,
                        CategoryId = yachtMaster.Id,
                        Quantity = 2,
                        Sold = 0,
                        Image = "b4a0b4ef-ed90-4e27-a92d-e377d5b01161_8th20c001-pb208-l.webp",
                        IsActive = true,
                        Model = "Yacht-Master 40",
                        ModelNumber = "126622",
                        Gender = "Men",
                        Condition = "Mint",
                        CaseMaterial = "Stainless Steel",
                        CaseSize = "40mm",
                        Crystal = "Sapphire",
                        DialColor = "Blue",
                        MovementType = "Automatic",
                        BraceletMaterial = "Stainless Steel"
                    },
					new ProductModel {
                        Name = "Rolex Submariner 16610",
                        Slug = "rolex-submariner-16610",
                        Description = "Classic Rolex Submariner with black dial and rotating bezel. Iconic dive watch with 300m water resistance.",
                        Price = 10795,
                        CapitalPrice = 7500,
                        BrandId = rolex.Id,
                        CategoryId = submariner.Id,
                        Quantity = 4,
                        Sold = 3,
                        Image = "f4763122-1b1c-4de9-bb0e-b52efc03dac7_8ts25c001-sg198-l-5-a.webp",
                        IsActive = true,
                        Model = "Submariner",
                        ModelNumber = "16610",
                        Gender = "Men",
                        Condition = "Excellent",
                        CaseMaterial = "Stainless Steel",
                        CaseSize = "40mm",
                        Crystal = "Sapphire",
                        DialColor = "Black",
                        MovementType = "Automatic",
                        BraceletMaterial = "Stainless Steel"
                    },
					new ProductModel {
                        Name = "Panerai Luminor 1950 3 Days Titanio DLC PAM00629",
                        Slug = "panerai-luminor-pam00629",
                        Description = "Panerai Luminor with black dial and DLC titanium case. Limited edition with 3-day power reserve.",
                        Price = 8500,
                        CapitalPrice = 6000,
                        BrandId = panerai.Id,
                        CategoryId = milgauss.Id, // Using milgauss category for now
                        Quantity = 2,
                        Sold = 0,
                        Image = "554a5be3-60e2-4cfd-bb59-84fd75428c46_8ts25c001-sb001-l-5.webp",
                        IsActive = true,
                        Model = "Luminor 1950",
                        ModelNumber = "PAM00629",
                        Gender = "Men",
                        Condition = "Mint",
                        CaseMaterial = "Titanium DLC",
                        CaseSize = "44mm",
                        Crystal = "Sapphire",
                        DialColor = "Black",
                        MovementType = "Manual",
                        BraceletMaterial = "Leather"
                    },
					new ProductModel {
                        Name = "Rolex Cosmograph Daytona 126508 Green Dial",
                        Slug = "rolex-daytona-126508",
                        Description = "Rolex Daytona with green dial and yellow gold case. Premium chronograph with tachymeter bezel.",
                        Price = 45000,
                        CapitalPrice = 35000,
                        BrandId = rolex.Id,
                        CategoryId = date.Id,
                        Quantity = 1,
                        Sold = 0,
                        Image = "5a6fcefd-c9f4-4b94-9f63-2a710dc6b310_8ts25c001-sb001-l-3.webp",
                        IsActive = true,
                        Model = "Daytona",
                        ModelNumber = "126508",
                        Gender = "Men",
                        Condition = "Like New",
                        CaseMaterial = "Yellow Gold",
                        CaseSize = "40mm",
                        Crystal = "Sapphire",
                        DialColor = "Green",
                        MovementType = "Automatic",
                        BraceletMaterial = "Yellow Gold"
                    },
					new ProductModel {
                        Name = "Mens Omega Speedmaster Chronoscope Stainless Steel",
                        Slug = "omega-speedmaster-chronoscope",
                        Description = "Omega Speedmaster Chronoscope with stainless steel case and chronograph functions. Professional racing chronometer.",
                        Price = 6500,
                        CapitalPrice = 4500,
                        BrandId = omega.Id,
                        CategoryId = oysterPerpetual.Id, // Using oyster category for now
                        Quantity = 6,
                        Sold = 2,
                        Image = "2e7999c8-54b3-4f7a-ac7d-37f59da8ae5d_8ts25c001-sb001-l-1-u.webp",
                        IsActive = true,
                        Model = "Speedmaster",
                        ModelNumber = "Chronoscope",
                        Gender = "Men",
                        Condition = "Good",
                        CaseMaterial = "Stainless Steel",
                        CaseSize = "43mm",
                        Crystal = "Sapphire",
                        DialColor = "Blue",
                        MovementType = "Manual",
                        BraceletMaterial = "Stainless Steel"
                    }
				);
				_context.SaveChanges();
			}
		}
		
		private static void CreateAdminUserAndRoles(DataContext _context)
		{
			// Create roles if they don't exist
			if (!_context.Roles.Any())
			{
				var roles = new List<IdentityRole>
				{
					new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
					new IdentityRole { Name = "Manager", NormalizedName = "MANAGER" },
					new IdentityRole { Name = "Staff", NormalizedName = "STAFF" },
					new IdentityRole { Name = "Sale", NormalizedName = "SALE" },
					new IdentityRole { Name = "Shipper", NormalizedName = "SHIPPER" },
					new IdentityRole { Name = "Customer", NormalizedName = "CUSTOMER" }
				};
				
				_context.Roles.AddRange(roles);
				_context.SaveChanges();
			}
			
			// Create admin user if it doesn't exist
			if (!_context.Users.Any(u => u.Email == "admin@arum.com"))
			{
				var adminUser = new AppUserModel
				{
					UserName = "admin",
					Email = "admin@arum.com",
					EmailConfirmed = true,
					PhoneNumber = "0123456789",
					FullName = "Administrator",
					Address = "Admin Address",
					Gender = "Male",
					Token = Guid.NewGuid().ToString()
				};
				
				_context.Users.Add(adminUser);
				_context.SaveChanges();
				
				// Add admin role to admin user
				var adminRole = _context.Roles.FirstOrDefault(r => r.Name == "Admin");
				if (adminRole != null)
				{
					var userRole = new IdentityUserRole<string>
					{
						UserId = adminUser.Id,
						RoleId = adminRole.Id
					};
					
					_context.UserRoles.Add(userRole);
					_context.SaveChanges();
				}
			}
		}
	}
}
