namespace Capstone.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CaloriesInDays",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Day = c.DateTime(nullable: false),
                        Calories = c.Int(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Foods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Storage = c.Int(nullable: false),
                        ExpiryDate = c.DateTime(),
                        CalculatedExpiryDate = c.DateTime(),
                        Quantity = c.Double(nullable: false),
                        QuantityClassifier = c.String(),
                        Vegan = c.Int(nullable: false),
                        Vegetarian = c.Int(nullable: false),
                        Ingredients = c.String(),
                        Traces = c.String(),
                        Calories = c.Int(nullable: false),
                        Favourite = c.Boolean(nullable: false),
                        Kitchen_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Kitchens", t => t.Kitchen_Id)
                .Index(t => t.Kitchen_Id);
            
            CreateTable(
                "dbo.Kitchens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(maxLength: 250),
                        Password = c.String(),
                        Vegetarian = c.Boolean(nullable: false),
                        Vegan = c.Boolean(nullable: false),
                        CalorieTracker = c.Boolean(nullable: false),
                        Allergies = c.String(),
                        EmailConfirmKey = c.Guid(nullable: false),
                        IsConfirmed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true, name: "UniqueEmail");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Kitchens", "User_Id", "dbo.Users");
            DropForeignKey("dbo.CaloriesInDays", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Foods", "Kitchen_Id", "dbo.Kitchens");
            DropIndex("dbo.Users", "UniqueEmail");
            DropIndex("dbo.Kitchens", new[] { "User_Id" });
            DropIndex("dbo.Foods", new[] { "Kitchen_Id" });
            DropIndex("dbo.CaloriesInDays", new[] { "User_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.Kitchens");
            DropTable("dbo.Foods");
            DropTable("dbo.CaloriesInDays");
        }
    }
}
