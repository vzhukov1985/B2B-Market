using System;
using System.Collections.Generic;
using Core.DBModels;

namespace Core.Models
{
    public class CurrentUserInfo
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsAdmin { get; set; }
        public string InitialPassword { get; set; }
        public CurrentUserClientInfo Client { get; set; }
        public string PasswordHash { get; set; }
        public string PinHash { get; set; }
        public bool UseBiometricAccess { get; set; }

        public List<Guid> FavoriteProductsIds { get; set; }
    }

    public class CurrentUserClientInfo
    {
        public Guid Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Bin { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonPhone { get; set; }
        public List<Guid> ContractedSuppliersIDs { get; set; }
    }
}
