using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface IFeatureRoleRepository
    {
        Task<IEnumerable<FeatureRole>> GetAllAsync();
        Task<FeatureRole> GetAsync(Guid Id);
        Task<IEnumerable<FeatureGroupRole>> GetFeatureGroupRolesAsync();
    }

    public class FeatureRoleRepository : DapperBaseRepository, IFeatureRoleRepository
    {
        private static readonly string TABLE = Tables.Name(Table.FeatureRoles);
        private static readonly string[] FIELDS = typeof(FeatureRole).DapperFields();

        public FeatureRoleRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<FeatureRole>> GetAllAsync()
        {
            return await QueryAsync<FeatureRole>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<IEnumerable<FeatureGroupRole>> GetFeatureGroupRolesAsync()
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
                FROM {TABLE} fr
                LEFT JOIN {Tables.Name(Table.Groups)} g
                ON fr.FeatureId = g.FeatureId
                LEFT JOIN {Tables.Name(Table.Features)} f
                ON f.Id = g.FeatureId";

            return await QueryAsync<FeatureGroupRole>(sqlTxt);
        }

        public async Task<FeatureRole> GetAsync(Guid Id)
        {
            return await QueryFirstOrDefaultAsync<FeatureRole>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }

    }
}
