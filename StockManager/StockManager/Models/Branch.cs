using ErrorOr;
using StockManager.Contracts.Branch;
using StockManager.ServiceErrors;

namespace StockManager.Models
{
    public class Branch
    {

        public const int MinNameLength = 3;
        public const int MaxNameLength = 50;

        public const int MinPhoneLength = 10;
        public const int MaxPhoneLength = 10;


        public Guid Id { get; }
        public string Name { get; }
        public string Town { get; }

        public string Phone { get; }

        public string Address { get; }

        public Branch(Guid id, string name, string town, string phone, string address)
        {
            Id = id;
            Name = name;
            Town = town;
            Phone = phone;
            Address = address;
        }

        public static ErrorOr<Branch> Create(
            string name,
            string town,
            string phone,
            string address,
            Guid? id = null)


        {
            List<Error> errors = new();

            if (name.Length is < MinNameLength or > MaxNameLength)
            {
                errors.Add(Errors.Branch.InvalidName);
            }

            if (phone.Length is < MinPhoneLength or > MaxPhoneLength)
            {
                errors.Add(Errors.Branch.InvalidPhone);
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return new Branch(
                id ?? Guid.NewGuid(),
                name,
                town,
                phone,
                address);
        }

        public static ErrorOr<Branch> From(CreateBranchRequest request)
        {
            return Create(
                request.Name,
                request.Town,
                request.Phone,
                request.Address
               );
        }

        public static ErrorOr<Branch> From(Guid id, UpsertBranchRequest request)
        {
            return Create(
                 request.Name,
                request.Town,
                request.Phone,
                request.Address,
                id);
        }


    }
}
