namespace Capstone.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class another : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Foods", "InitialQuantity", c => c.Double(nullable: false));
            AddColumn("dbo.Foods", "InitialQuantityClassifier", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Foods", "InitialQuantityClassifier");
            DropColumn("dbo.Foods", "InitialQuantity");
        }
    }
}
