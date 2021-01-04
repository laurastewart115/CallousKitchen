namespace Capstone.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shoppinglist : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Foods", "OnShoppingList", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Foods", "OnShoppingList");
        }
    }
}
