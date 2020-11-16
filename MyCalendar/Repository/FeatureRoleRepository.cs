using Dapper;
using DFM.Utils;
using MyCalendar.Enums;
using MyCalendar.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCalendar.Repository
{
    public interface IFeatureRoleRepository
    {
        Task<IEnumerable<FeatureRole>> GetAllAsync();
        Task<FeatureRole> GetAsync(Guid Id);
        Task<IEnumerable<FeatureGroupRole>> GetFeatureGroupRolesAsync();
    }

    public class FeatureRoleRepository : IFeatureRoleRepository
    {
        private readonly Func<IDbConnection> dbConnectionFactory;
        private static readonly string TABLE = "FeatureRoles";
        private static readonly string[] FIELDS = typeof(FeatureRole).DapperFields();

        public FeatureRoleRepository(Func<IDbConnection> dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<FeatureRole>> GetAllAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<FeatureRole>($"{DapperHelper.SELECT(TABLE, FIELDS)}")).ToArray();
            }
        }

        public async Task<IEnumerable<FeatureGroupRole>> GetFeatureGroupRolesAsync()
        {
            using (var sql = dbConnectionFactory())
            {
                string sqlTxt = $@"
                    SELECT 
	                    fr.RoleIds, 
	                    fr.FeatureId, 
	                    fr.Name AS FeatureRoleName, 
	                    fr.ReadRight, 
	                    fr.SaveRight, 
	                    fr.DeleteRight, 
	                    fr.FullRights, 
	                    f.Name AS FeatureName, 
	                    g.Name AS GroupName,
                        g.Id As GroupId
                    FROM FeatureRoles fr
                    LEFT JOIN Groups g
                    ON fr.FeatureId = g.FeatureId
                    LEFT JOIN Features f
                    ON f.Id = g.FeatureId";

                return (await sql.QueryAsync<FeatureGroupRole>(sqlTxt)).ToArray();
            }
        }

        public async Task<FeatureRole> GetAsync(Guid Id)
        {
            using (var sql = dbConnectionFactory())
            {
                return (await sql.QueryAsync<FeatureRole>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id })).FirstOrDefault();
            }
        }

    }
}
