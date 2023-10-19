using CM.Core.Domain;
using CM.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM.Data.DBInitializer
{
    public partial class MongoDbInitializer
    {
        private readonly IMongoRepoistory<Company> _companyRepo;
        private readonly IMongoRepoistory<Contact> _contactRepo;
        public MongoDbInitializer(IMongoRepoistory<Company> companyRepo,
            IMongoRepoistory<Contact> contactRepo)
        {
            _companyRepo = companyRepo;
            _contactRepo = contactRepo;
        }
        public async Task Seed()
        {
            await SeedCompany();
            await SeedContact();
        }
        private async Task SeedCompany()
        {
            var list = new List<Company>();
            if (!await _companyRepo.AnyAsync())
            {
                var random= new Random();
                for (int i = 0; i < 10; i++)
                {
                    list.Add(new Company
                    {
                        Name = $"Company{i}",
                        NumberOfEmployees= random.Next(),
                    });
                }

                await _companyRepo.InsertRangeAsync(list);
            }
        }
        private async Task SeedContact()
        {
            var list = new List<Contact>();
            if (!await _contactRepo.AnyAsync())
            {
                for (int i = 0; i < 10; i++)
                {
                    list.Add(new Contact
                    {
                        Name = $"Contact{i}",
                        
                    });
                }

                await _contactRepo.InsertRangeAsync(list);
            }
        }
    }
}
