using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Skybrud.Umbraco.Redirects.Extensions;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Skybrud.Umbraco.Redirects.Models.Import.Db
{
    public interface IBulkInsertProvider
    {
        /// <summary>
        /// Imports all redirects into database. They must be validated items.
        /// </summary>
        /// <param name="redirectItems"></param>
        void BulkImport(IEnumerable<ValidatedRedirectItem> redirectItems);
    }

    /// <summary>
    /// This uses the SqlBulkCopy method of inserting rows into SQL Server. This seems to be the fastest method in C#. These are swappable
    /// allowing for different bulk insert approaches and other databases?
    /// </summary>
    public class SqlServerBulkCopy : IBulkInsertProvider
    {
        /// <summary>
        /// Imports all redirects into database. They must be validated items.
        /// </summary>
        /// <param name="redirectItems"></param>
        public void BulkImport(IEnumerable<ValidatedRedirectItem> redirectItems)
        {
            var dataTable = redirectItems
                .Where(validatedItem => validatedItem.IsValid)
                .Select(validatedItem => validatedItem.Item.Dto)
                .ToList()
                .ToDataTable();

            //Slight hack
            dataTable.Columns["UniqueId"].ColumnName = "RedirectUniqueId";
            dataTable.Columns["Id"].ColumnName = "RedirectId";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString))
            {
                connection.Open();
    
                var truncate = new SqlCommand
                {
                    CommandText = @"TRUNCATE TABLE SkybrudRedirectsImport",
                    CommandType = CommandType.Text,
                    Connection = connection
                };

                truncate.ExecuteScalar();

                using (var bulkCopy = new SqlBulkCopy
                (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                ))
                {
                    bulkCopy.DestinationTableName = "SkybrudRedirectsImport";

                    bulkCopy.BatchSize = 0;

                    foreach (DataColumn col in dataTable.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }

                    bulkCopy.WriteToServer(dataTable);
                    bulkCopy.Close();
                }
                connection.Close();
                connection.Open();

                var cmd = new SqlCommand();

                cmd.CommandText = @"
                MERGE INTO SkybrudRedirects WITH (HOLDLOCK) AS target
                USING SkybrudRedirectsImport AS source
                    ON target.LinkUrl = source.LinkUrl
                    AND target.Url = source.Url
                    AND target.QueryString = source.QueryString
                WHEN MATCHED THEN UPDATE SET 
                    target.QueryString = source.QueryString,
                    target.LinkMode = source.LinkMode,
                    target.LinkId = source.LinkId,
                    target.Created = source.Created,
                    target.Updated = source.Updated,
                    target.IsPermanent = source.IsPermanent,
                    target.IsRegex = source.IsRegex,
                    target.ForwardQueryString = source.ForwardQueryString

                WHEN NOT MATCHED BY TARGET THEN

                INSERT(
                    Url,
                    QueryString,
                    RedirectUniqueId,
                    RootNodeId,
                    LinkMode,
                    LinkId,
                    LinkUrl,
                    LinkName,
                    Created,
                    Updated,
                    IsPermanent,
                    IsRegEx,
                    ForwardQueryString)
                VALUES(
                    source.Url, 
                    source.QueryString,
                    source.RedirectUniqueId,
                    source.RootNodeId,
                    source.LinkMode,
                    source.LinkId,
                    source.LinkUrl,
                    source.LinkName,
                    source.Created,
                    source.Updated,
                    source.IsPermanent,
                    source.IsRegEx,
                    source.ForwardQueryString);";

                cmd.CommandType = CommandType.Text;
                cmd.Connection = connection;

                cmd.ExecuteScalar();

                connection.Close();
            }

            dataTable.Clear();
        }
    }
}