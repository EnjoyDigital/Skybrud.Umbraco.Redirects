using Skybrud.Umbraco.Redirects.Models;
using Umbraco.Core.Migrations;

namespace Skybrud.Umbraco.Redirects.Migrations
{

    public class CreateImportTable : MigrationBase {

        public CreateImportTable(IMigrationContext context) : base(context) { }

        public override void Migrate() {            
            if (TableExists(RedirectItemRowImport.TableName)) return;
            Create.Table<RedirectItemRowImport>().Do();
        }
    }
}