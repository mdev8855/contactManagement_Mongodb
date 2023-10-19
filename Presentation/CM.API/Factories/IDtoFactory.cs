using CM.API.Models.Dto;
using CM.Core.Domain;
using CM.Services.Companies;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace CM.API.Factories
{
    public interface IDtoFactory
    {
        ContactDto PrepareContactDto(Contact contact);
        CompanyDto PrepareCompanyDto(Company company);
        Task<List<CompanyDto>> PrepareListOfCompanyDto(List<string> companyIds);
    }
    public class DtoFactory : IDtoFactory
    {
        private readonly ICompanyService _companyService;
        public DtoFactory(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        public ContactDto PrepareContactDto(Contact contact)
        {
            if (contact == null)
                return null;

            var dto = new ContactDto
            {
                Id = contact.Id,
                Name = contact.Name,
                Companies = PrepareListOfCompanyDto(contact.Companies).Result,
                DynamicFields = contact?.DynamicFields?.ToDictionary(),
            };

            return dto;
        }

        public CompanyDto PrepareCompanyDto(Company company)
        {
            if (company == null)
                return null;

            var dto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                NumberOfEmployees = company.NumberOfEmployees,
            };
            return dto;
        }
        public async Task<List<CompanyDto>> PrepareListOfCompanyDto(List<string> companyIds)
        {
            if (companyIds.Any())
            {
                var companies = await _companyService.ListAllAsyncByIds(companyIds);
                if (companies.Any())
                    return companies.Select(x => PrepareCompanyDto(x)).ToList();
            }

            return Enumerable.Empty<CompanyDto>().ToList();

        }

    }
}
