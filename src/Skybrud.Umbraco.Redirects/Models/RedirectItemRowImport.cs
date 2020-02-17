using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Skybrud.Umbraco.Redirects.Models
{

    [TableName(TableName)]
    [PrimaryKey(PrimaryKey, AutoIncrement = true)]
    [ExplicitColumns]
    public class RedirectItemRowImport {

        #region Constants

        public const string TableName = "SkybrudRedirectsImport";

        public const string PrimaryKey = "Id";

        #endregion

        #region Properties

        [Column(PrimaryKey)]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Key")]
        public string Key { get; set; }

        [Column("RootId")]
        public int RootId { get; set; }

        [Column("Url")]
        public string Url { get; set; }

        [Column("QueryString")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string QueryString { get; set; }

        [Column("DestinationType")]
        public string DestinationType { get; set; }

        [Column("DestinationId")]
        public int DestinationId { get; set; }

        [Column("DestinationKey")]
        public string DestinationKey { get; set; }

        [Column("DestinationUrl")]
        public string DestinationUrl { get; set; }

        [Column("Created")]
        public long Created { get; set; }

        [Column("Updated")]
        public long Updated { get; set; }

		[Column("IsPermanent")]
		public bool IsPermanent { get; set; }

		[Column("IsRegex")]
		public bool IsRegex { get; set; }

		[Column("ForwardQueryString")]
		public bool ForwardQueryString { get; set; }

		#endregion

	}

}