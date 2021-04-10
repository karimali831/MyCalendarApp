using DFM.Utils;
using Appology.Enums;
using Appology.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Appology.Repository
{
    public interface IFeatureRepository
    {
        Task<IEnumerable<Feature>> GetAllAsync();
        Task<Feature> GetAsync(int Id);
    }

    public class FeatureRepository : DapperBaseRepository, IFeatureRepository
    {
        private static readonly string TABLE = Tables.Name(Table.Features);
        private static readonly string[] FIELDS = typeof(Feature).DapperFields();

        public FeatureRepository(Func<IDbConnection> dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<IEnumerable<Feature>> GetAllAsync()
        {
            return await QueryAsync<Feature>($"{DapperHelper.SELECT(TABLE, FIELDS)}");
        }

        public async Task<Feature> GetAsync(int Id)
        {
            return await QueryFirstOrDefaultAsync<Feature>($"{DapperHelper.SELECT(TABLE, FIELDS)} WHERE Id = @Id", new { Id });
        }

    }
}
