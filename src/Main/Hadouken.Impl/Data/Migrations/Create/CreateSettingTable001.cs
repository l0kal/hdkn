using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

namespace Hadouken.Impl.Data.Migrations.Create
{
    public class CreateSettingTable001 : DbMigration
    {
        public override void Up()
        {
            CreateTable("Setting", c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(maxLength: 200),
                    Value = c.String(),
                    Type = c.String()
                });
        }
    }
}
