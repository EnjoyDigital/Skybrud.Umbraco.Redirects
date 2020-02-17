using Skybrud.Umbraco.Redirects.Models;
using Skybrud.Umbraco.Redirects.Models.Database;
using Umbraco.Core.Migrations;

namespace Skybrud.Umbraco.Redirects.Migrations {

    public class CreateTable : MigrationBase {

        public CreateTable(IMigrationContext context) : base(context) { }

        public override void Migrate() {
            if (TableExists(RedirectItemSchema.TableName)) return;
            Create.Table<RedirectItemSchema>().Do();

            if (TableExists(RedirectItemRow.TableName)) return;
            Create.Table<RedirectItemRow>().Do();
            
            if (TableExists(RedirectItemRowImport.TableName)) return;
            Create.Table<RedirectItemRowImport>().Do();
        }

    }

}